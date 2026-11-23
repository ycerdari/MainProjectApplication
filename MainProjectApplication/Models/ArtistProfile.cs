using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PUPApp.Models
{
    public class ArtistProfile
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        [Display(Name = "Stage name")]
        public string StageName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        public string? College { get; set; }

        public string? PhotoPath { get; set; }

        // An admin moves a new artist from Pending to Approved (or Rejected).
        public ArtistStatus Status { get; set; } = ArtistStatus.Pending;

        [StringLength(300)]
        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<Track> Tracks { get; set; } = new List<Track>();

        // Convenience for C# code only. In LINQ queries use: a.Status == ArtistStatus.Approved
        [NotMapped]
        public bool IsApproved => Status == ArtistStatus.Approved;
    }
}
