using System.ComponentModel.DataAnnotations;

namespace PUPApp.ViewModels.Validation
{
    // Custom data annotation: [MaxFileSize(10 * 1024 * 1024)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxBytes;

        public MaxFileSizeAttribute(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxBytes)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"File must be {_maxBytes / (1024 * 1024)} MB or smaller.");
            }
            return ValidationResult.Success;
        }
    }
}
