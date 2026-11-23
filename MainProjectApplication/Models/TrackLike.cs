namespace PUPApp.Models
{
    // Join table: many-to-many between User and Track.
    // Named TrackLike (not Like) to avoid confusion with EF.Functions.Like.
    public class TrackLike
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int TrackId { get; set; }
        public Track? Track { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
