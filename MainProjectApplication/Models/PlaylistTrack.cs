namespace PUPApp.Models
{
    // Join table: many-to-many between Playlist and Track.
    // The composite primary key (PlaylistId + TrackId) is set in ApplicationDbContext.
    public class PlaylistTrack
    {
        public int PlaylistId { get; set; }
        public Playlist? Playlist { get; set; }

        public int TrackId { get; set; }
        public Track? Track { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
