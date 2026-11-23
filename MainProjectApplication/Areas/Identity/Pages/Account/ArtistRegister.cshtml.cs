#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUPApp.Data;
using PUPApp.Models;

namespace PUPApp.Areas.Identity.Pages.Account
{
    public class ArtistRegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArtistRegisterModel> _logger;

        public ArtistRegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<ArtistRegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, StringLength(60, MinimumLength = 2)]
            [Display(Name = "Stage name")]
            public string StageName { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, StringLength(100, MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [StringLength(100)]
            public string College { get; set; }

            [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm that you only upload original music.")]
            [Display(Name = "I only upload original music that I own the rights to")]
            public bool ConfirmOriginal { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = new ApplicationUser { DisplayName = Input.StageName.Trim() };
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            // The account starts as Pending. An admin approves it, and only then does the Artist role unlock uploads.
            _context.ArtistProfiles.Add(new ArtistProfile
            {
                UserId = user.Id,
                StageName = user.DisplayName,
                College = string.IsNullOrWhiteSpace(Input.College) ? null : Input.College,
                Status = ArtistStatus.Pending
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("A new artist account was created and is pending review.");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect("~/Artist");   // shows the "pending review" page
        }
    }
}
