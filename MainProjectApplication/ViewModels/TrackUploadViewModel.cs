using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PUPApp.Models;
using PUPApp.ViewModels.Validation;

namespace PUPApp.ViewModels
{
    // Used by the artist Create and Edit forms.
    public class TrackUploadViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Track title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(80)]
        [Display(Name = "Featured artists (optional)")]
        public string? FeaturedArtists { get; set; }

        [Display(Name = "Release type")]
        public ReleaseType ReleaseType { get; set; } = ReleaseType.Single;

        [StringLength(40)]
        [Display(Name = "Specify release type (optional)")]
        public string? ReleaseTypeOther { get; set; }

        [StringLength(80)]
        [Display(Name = "Release or album title (optional)")]
        public string? ReleaseTitle { get; set; }

        // The ids of the genres the artist ticked (1 to 5)
        public List<int> GenreIds { get; set; } = new();

        [StringLength(40)]
        [Display(Name = "Specify genre (optional)")]
        public string? OtherGenre { get; set; }

        [Display(Name = "Contains explicit lyrics")]
        public bool IsExplicit { get; set; }

        public TrackVisibility Visibility { get; set; } = TrackVisibility.Public;

        [Display(Name = "Audio file")]
        [AudioFile]
        [MaxFileSize(100 * 1024 * 1024, ErrorMessage = "The file must be 100 MB or smaller.")]
        public IFormFile? AudioFile { get; set; }

        // Filled in by JavaScript when a file is chosen
        public int DurationSeconds { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm that you own the rights to this music.")]
        [Display(Name = "I own or control the rights to this music")]
        public bool ConfirmRights { get; set; }

        public string? CurrentAudioPath { get; set; }
        public string? CurrentFileName { get; set; }

        [ValidateNever]
        public List<Genre> AllGenres { get; set; } = new();
    }

    public class ArtistDashboardViewModel
    {
        public ArtistProfile Artist { get; set; } = null!;
        public int TrackCount { get; set; }
        public int PublicCount { get; set; }
        public long TotalPlays { get; set; }
        public int FavoriteCount { get; set; }
        public List<Track> Recent { get; set; } = new();
    }
}
