using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PUPApp.Data;
using PUPApp.Models;

namespace PUPApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArtistsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List artists by status: Pending (default), Approved, Rejected
        public async Task<IActionResult> Index(ArtistStatus status = ArtistStatus.Pending)
        {
            ViewBag.Status = status;
            var artists = await _context.ArtistProfiles.Include(a => a.User)
                .Where(a => a.Status == status)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
            return View(artists);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var artist = await _context.ArtistProfiles.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (artist?.User == null) return NotFound();

            artist.Status = ArtistStatus.Approved;
            artist.ReviewedAt = DateTime.UtcNow;
            artist.ReviewNote = null;
            await _context.SaveChangesAsync();

            // The Artist role is what unlocks the upload area
            if (!await _userManager.IsInRoleAsync(artist.User, "Artist"))
            {
                await _userManager.AddToRoleAsync(artist.User, "Artist");
            }

            TempData["Success"] = $"{artist.StageName} was approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var artist = await _context.ArtistProfiles.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
            if (artist?.User == null) return NotFound();

            artist.Status = ArtistStatus.Rejected;
            artist.ReviewedAt = DateTime.UtcNow;
            artist.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            await _context.SaveChangesAsync();

            if (await _userManager.IsInRoleAsync(artist.User, "Artist"))
            {
                await _userManager.RemoveFromRoleAsync(artist.User, "Artist");   // also covers revoking
            }

            TempData["Success"] = $"{artist.StageName} was rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
