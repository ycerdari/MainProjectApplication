using PUPApp.Models;

namespace PUPApp.Data
{
    // One place that lists every genre. The database is seeded from this list.
    public static class GenreCatalog
    {
        public static readonly string[] Names =
        {
            "Pop", "Rock", "Hip-Hop/Rap", "R&B/Soul", "OPM", "Alternative", "Indie", "Electronic",
            "Dance/EDM", "House", "Techno", "Trance", "Drum & Bass", "Dubstep", "Country", "Folk",
            "Acoustic", "Singer-Songwriter", "Jazz", "Blues", "Classical", "Opera", "Reggae", "Ska",
            "Metal", "Punk", "Funk", "Disco", "Gospel/Christian", "Latin", "K-Pop", "J-Pop", "C-Pop",
            "World", "Soundtrack/Score", "Lo-fi", "Ambient", "New Age", "Experimental", "Instrumental",
            "Spoken Word", "Kids", "Holiday", "Others"   // keep "Others" last
        };

        public static int OthersId => Names.Length;

        public static IEnumerable<Genre> Seed() =>
            Names.Select((name, i) => new Genre { Id = i + 1, Name = name });
    }
}
