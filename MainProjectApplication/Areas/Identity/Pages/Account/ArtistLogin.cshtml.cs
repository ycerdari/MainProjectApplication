#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PUPApp.Data;
using PUPApp.Models;

namespace PUPApp.Areas.Identity.Pages.Account
{
    public class ArtistLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ArtistLoginModel(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Stay signed in")]
            public bool RememberMe { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = "~/Artist", RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Correct password. Make sure this really is an artist account.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            var isArtist = user != null && await _context.ArtistProfiles.AnyAsync(a => a.UserId == user.Id);
            if (!isArtist)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty,
                    "This account isn't an artist account. Use the listener sign-in, or create an artist account.");
                return Page();
            }

            return LocalRedirect("~/Artist");
        }
    }
}
