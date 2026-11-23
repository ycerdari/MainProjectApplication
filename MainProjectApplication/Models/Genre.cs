using System.ComponentModel.DataAnnotations;

namespace PUPApp.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}
