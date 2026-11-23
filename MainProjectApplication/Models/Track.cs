using System.ComponentModel.DataAnnotations;

namespace PUPApp.Models
{
    public class Track
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(80)]
        public string? FeaturedArtists { get; set; }

        // The audio file lives in wwwroot/uploads; only its path is stored here.
        [Required]
        public string AudioPath { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }

        [StringLength(10)]
        public string AudioFormat { get; set; } = string.Empty;   // mp3, wav, flac, ...
        public long FileSizeBytes { get; set; }

        public int DurationSeconds { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int PlayCount { get; set; }

        public ReleaseType ReleaseType { get; set; } = ReleaseType.Single;

        [StringLength(40)]
        public string? ReleaseTypeOther { get; set; }   // used when ReleaseType is Other

        [StringLength(40)]
        public string? OtherGenre { get; set; }         // used when the "Others" genre is picked

        public bool IsExplicit { get; set; }
        public TrackVisibility Visibility { get; set; } = TrackVisibility.Public;

        public int ArtistProfileId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }

        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        // Many-to-many: a track can have several genres (EF creates the join table).
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
        public ICollection<TrackLike> Likes { get; set; } = new List<TrackLike>();
    }
}
