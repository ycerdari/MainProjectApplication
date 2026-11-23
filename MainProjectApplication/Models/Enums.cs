using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PUPApp.Models
{
    public enum ArtistStatus { Pending, Approved, Rejected }

    public enum TrackVisibility { Public, Unlisted, Draft }

    public enum ReleaseType
    {
        Single,
        EP,
        Album,
        Mixtape,
        Compilation,
        [Display(Name = "Live album")] LiveAlbum,
        Remix,
        [Display(Name = "Soundtrack/Score")] Soundtrack,
        Demo,
        Cover,
        Instrumental,
        Karaoke,
        [Display(Name = "Spoken word")] SpokenWord,
        [Display(Name = "Others")] Other
    }

    public static class EnumExtensions
    {
        // Returns the [Display(Name=...)] text of an enum value, or its plain name.
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
        }
    }
}
