using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUPApp.Data;
using PUPApp.Models;
using PUPApp.Services;
using PUPApp.ViewModels;

namespace PUPApp.Areas.Artist.Controllers
{
    [Area("Artist")]
    [Authorize(Roles = "Artist")]   // the Artist role is given when an admin approves the account
    public class TracksController : Controller
    {
        private const long MaxRequestBytes = 105 * 1024 * 1024; // 100 MB file + form overhead
        private const int MaxGenres = 5;

        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _storage;

        public TracksController(ApplicationDbContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        // ---------- READ: my tracks ----------
        public async Task<IActionResult> Index()
        {
            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            var tracks = await _context.Tracks
                .Include(t => t.Genres).Include(t => t.Album)
                .Where(t => t.ArtistProfileId == artist.Id)
                .OrderByDescending(t => t.UploadedAt)
                .ToListAsync();
            return View(tracks);
        }

        // ---------- CREATE (upload) ----------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await GetArtistAsync() == null) return NotApprovedYet();
            var vm = new TrackUploadViewModel();
            await LoadGenresAsync(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
        public async Task<IActionResult> Create(TrackUploadViewModel vm)
        {
            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            if (vm.AudioFile == null)
            {
                ModelState.AddModelError(nameof(vm.AudioFile), "Please choose an audio file.");
            }
            var genres = await ValidateGenresAsync(vm);

            if (!ModelState.IsValid)
            {
                await LoadGenresAsync(vm);
                return View(vm);
            }

            var file = vm.AudioFile!;
            var path = await _storage.SaveAsync(file, "audio");

            var track = new Track
            {
                Title = vm.Title.Trim(),
                FeaturedArtists = Clean(vm.FeaturedArtists),
                AudioPath = path,
                OriginalFileName = Path.GetFileName(file.FileName),
                AudioFormat = FormatOf(file),
                FileSizeBytes = file.Length,
                DurationSeconds = vm.DurationSeconds,
                ReleaseType = vm.ReleaseType,
                ReleaseTypeOther = vm.ReleaseType == ReleaseType.Other ? Clean(vm.ReleaseTypeOther) : null,
                IsExplicit = vm.IsExplicit,
                Visibility = vm.Visibility,
                ArtistProfileId = artist.Id,
                Album = await FindOrCreateAlbumAsync(artist, vm),
                Genres = genres,
                OtherGenre = genres.Any(g => g.Id == GenreCatalog.OthersId) ? Clean(vm.OtherGenre) : null
            };

            try
            {
                _context.Tracks.Add(track);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _storage.Delete(path);   // don't leave an orphan file
                throw;
            }

            TempData["Success"] = track.Visibility == TrackVisibility.Public
                ? "Your track is live and now appears in Browse and Search."
                : "Your track was saved. It is not listed publicly.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- UPDATE ----------
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            var track = await _context.Tracks.Include(t => t.Genres).Include(t => t.Album)
                .FirstOrDefaultAsync(t => t.Id == id && t.ArtistProfileId == artist.Id);
            if (track == null) return NotFound();   // also covers "not your track"

            var vm = new TrackUploadViewModel
            {
                Id = track.Id,
                Title = track.Title,
                FeaturedArtists = track.FeaturedArtists,
                ReleaseType = track.ReleaseType,
                ReleaseTypeOther = track.ReleaseTypeOther,
                ReleaseTitle = track.Album?.Title,
                GenreIds = track.Genres.Select(g => g.Id).ToList(),
                OtherGenre = track.OtherGenre,
                IsExplicit = track.IsExplicit,
                Visibility = track.Visibility,
                DurationSeconds = track.DurationSeconds,
                CurrentAudioPath = track.AudioPath,
                CurrentFileName = track.OriginalFileName,
                ConfirmRights = true
            };
            await LoadGenresAsync(vm);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(MaxRequestBytes)]
        public async Task<IActionResult> Edit(int id, TrackUploadViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            var track = await _context.Tracks.Include(t => t.Genres)
                .FirstOrDefaultAsync(t => t.Id == id && t.ArtistProfileId == artist.Id);
            if (track == null) return NotFound();

            ModelState.Remove(nameof(vm.ConfirmRights));   // already confirmed when it was uploaded
            var genres = await ValidateGenresAsync(vm);

            if (!ModelState.IsValid)
            {
                vm.CurrentAudioPath = track.AudioPath;
                vm.CurrentFileName = track.OriginalFileName;
                await LoadGenresAsync(vm);
                return View(vm);
            }

            track.Title = vm.Title.Trim();
            track.FeaturedArtists = Clean(vm.FeaturedArtists);
            track.ReleaseType = vm.ReleaseType;
            track.ReleaseTypeOther = vm.ReleaseType == ReleaseType.Other ? Clean(vm.ReleaseTypeOther) : null;
            track.IsExplicit = vm.IsExplicit;
            track.Visibility = vm.Visibility;
            track.Album = await FindOrCreateAlbumAsync(artist, vm);
            track.OtherGenre = genres.Any(g => g.Id == GenreCatalog.OthersId) ? Clean(vm.OtherGenre) : null;

            track.Genres.Clear();
            foreach (var g in genres) track.Genres.Add(g);

            string? oldPath = null, newPath = null;
            if (vm.AudioFile != null)       // the artist chose to replace the audio
            {
                newPath = await _storage.SaveAsync(vm.AudioFile, "audio");
                oldPath = track.AudioPath;
                track.AudioPath = newPath;
                track.OriginalFileName = Path.GetFileName(vm.AudioFile.FileName);
                track.AudioFormat = FormatOf(vm.AudioFile);
                track.FileSizeBytes = vm.AudioFile.Length;
                track.DurationSeconds = vm.DurationSeconds;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _storage.Delete(newPath);
                throw;
            }
            _storage.Delete(oldPath);       // only after the database update worked

            TempData["Success"] = "Track updated.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Publish / hide ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id && t.ArtistProfileId == artist.Id);
            if (track == null) return NotFound();

            track.Visibility = track.Visibility == TrackVisibility.Public ? TrackVisibility.Draft : TrackVisibility.Public;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------- DELETE ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await GetArtistAsync();
            if (artist == null) return NotApprovedYet();

            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == id && t.ArtistProfileId == artist.Id);
            if (track == null) return NotFound();

            // These two use DeleteBehavior.Restrict, so remove them first
            _context.TrackLikes.RemoveRange(await _context.TrackLikes.Where(l => l.TrackId == id).ToListAsync());
            _context.PlaylistTracks.RemoveRange(await _context.PlaylistTracks.Where(p => p.TrackId == id).ToListAsync());

            var path = track.AudioPath;
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
            _storage.Delete(path);

            TempData["Success"] = "Track deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private async Task<ArtistProfile?> GetArtistAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.ArtistProfiles
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == ArtistStatus.Approved);
        }

        private IActionResult NotApprovedYet() => RedirectToAction("Index", "Dashboard");

        private async Task LoadGenresAsync(TrackUploadViewModel vm)
        {
            vm.AllGenres = await _context.Genres.OrderBy(g => g.Id).ToListAsync();
        }

        // 1 to 5 genres, and every id must exist. Returns the genres to attach.
        private async Task<List<Genre>> ValidateGenresAsync(TrackUploadViewModel vm)
        {
            var ids = vm.GenreIds.Distinct().ToList();
            if (ids.Count == 0)
                ModelState.AddModelError(nameof(vm.GenreIds), "Pick at least one genre.");
            else if (ids.Count > MaxGenres)
                ModelState.AddModelError(nameof(vm.GenreIds), $"You can pick up to {MaxGenres} genres.");

            var genres = await _context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
            if (genres.Count != ids.Count)
                ModelState.AddModelError(nameof(vm.GenreIds), "One of the selected genres is invalid.");
            return genres;
        }

        // A release title (for anything but a single) becomes an Album row, reused next time.
        private async Task<Album?> FindOrCreateAlbumAsync(ArtistProfile artist, TrackUploadViewModel vm)
        {
            var title = Clean(vm.ReleaseTitle);
            if (title == null || vm.ReleaseType == ReleaseType.Single) return null;

            return await _context.Albums.FirstOrDefaultAsync(a => a.ArtistProfileId == artist.Id && a.Title == title)
                ?? new Album { Title = title, ArtistProfileId = artist.Id, ReleaseDate = DateTime.Today };
        }

        private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        private static string FormatOf(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
            return ext.Length is > 0 and <= 10 ? ext : "audio";
        }
    }
}
