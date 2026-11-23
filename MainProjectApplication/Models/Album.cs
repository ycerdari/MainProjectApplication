using System.ComponentModel.DataAnnotations;

namespace PUPApp.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        public string? CoverPath { get; set; }

        // One artist has many albums
        public int ArtistProfileId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }

        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}
