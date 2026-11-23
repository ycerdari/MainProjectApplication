using System.ComponentModel.DataAnnotations;

namespace PUPApp.ViewModels.Validation
{
    // Accepts MP3, M4A, WAV, FLAC, AAC, OGG, OPUS, AIFF, WMA, RAW and any file the browser reports as audio/*.
    public class AudioFileAttribute : ValidationAttribute
    {
        public static readonly string[] Extensions =
        {
            ".mp3", ".m4a", ".wav", ".flac", ".aac", ".ogg", ".oga", ".opus",
            ".aiff", ".aif", ".wma", ".raw", ".pcm", ".alac", ".ape", ".amr"
        };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var isAudioType = (file.ContentType ?? "").StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

                if (!Extensions.Contains(ext) && !isAudioType)
                {
                    return new ValidationResult(ErrorMessage ??
                        "That file type isn't supported. Use MP3, M4A, WAV, FLAC, AAC, OGG, OPUS, AIFF, WMA, RAW or another audio format.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
