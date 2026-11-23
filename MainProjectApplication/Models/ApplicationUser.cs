using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PUPApp.Models
{
    // Extends the built-in Identity user so we can add our own fields.
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ArtistProfile? ArtistProfile { get; set; }          // one-to-one (optional)
        public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public ICollection<TrackLike> Likes { get; set; } = new List<TrackLike>();
    }
}
