using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PUPApp.Data;
using PUPApp.Models;

namespace PUPApp.Controllers
{
    // Listener side: browse and search. Uploading now lives in the Artist area.
    [Authorize]
    public class TracksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TracksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int? genreId, string? college, ReleaseType? type)
        {
            // Only PUBLIC tracks from APPROVED artists are ever shown
            var query = _context.Tracks
                .Include(t => t.ArtistProfile).Include(t => t.Genres).Include(t => t.Album)
                .Where(t => t.Visibility == TrackVisibility.Public
                         && t.ArtistProfile!.Status == ArtistStatus.Approved);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Title.Contains(search)
                    || t.ArtistProfile!.StageName.Contains(search)
                    || (t.FeaturedArtists != null && t.FeaturedArtists.Contains(search))
                    || (t.OtherGenre != null && t.OtherGenre.Contains(search))
                    || t.Genres.Any(g => g.Name.Contains(search)));
            }
            if (genreId.HasValue) query = query.Where(t => t.Genres.Any(g => g.Id == genreId.Value));
            if (!string.IsNullOrWhiteSpace(college)) query = query.Where(t => t.ArtistProfile!.College == college);
            if (type.HasValue) query = query.Where(t => t.ReleaseType == type.Value);

            ViewBag.Search = search;
            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId);
            ViewBag.Colleges = new SelectList(
                await _context.ArtistProfiles.Where(a => a.College != null).Select(a => a.College!).Distinct().OrderBy(c => c).ToListAsync(),
                college);
            ViewBag.Types = Html.TypeList(type);

            var tracks = await query.OrderByDescending(t => t.UploadedAt).ToListAsync();
            return View(tracks);
        }

        private static class Html
        {
            public static SelectList TypeList(ReleaseType? selected) =>
                new SelectList(Enum.GetValues<ReleaseType>().Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text", selected);
        }
    }
}
