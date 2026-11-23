using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUPApp.Data;
using PUPApp.Models;
using PUPApp.ViewModels;

namespace PUPApp.Areas.Artist.Controllers
{
    [Area("Artist")]
    [Authorize] // any signed-in user; pending artists see the "pending review" page
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.ArtistProfiles.FirstOrDefaultAsync(a => a.UserId == userId);

            if (artist == null)
            {
                // Signed in, but not an artist account
                return RedirectToPage("/Account/ArtistRegister", new { area = "Identity" });
            }

            if (artist.Status != ArtistStatus.Approved)
            {
                return View("Pending", artist);
            }

            var tracks = _context.Tracks.Where(t => t.ArtistProfileId == artist.Id);

            var vm = new ArtistDashboardViewModel
            {
                Artist = artist,
                TrackCount = await tracks.CountAsync(),
                PublicCount = await tracks.CountAsync(t => t.Visibility == TrackVisibility.Public),
                TotalPlays = await tracks.SumAsync(t => (long?)t.PlayCount) ?? 0,
                FavoriteCount = await _context.TrackLikes.CountAsync(l => l.Track!.ArtistProfileId == artist.Id),
                Recent = await tracks.Include(t => t.Genres)
                                     .OrderByDescending(t => t.UploadedAt).Take(5).ToListAsync()
            };
            return View(vm);
        }
    }
}
