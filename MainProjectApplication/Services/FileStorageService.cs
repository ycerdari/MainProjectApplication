namespace PUPApp.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(IFormFile file, string subfolder)
        {
            // Never trust the uploaded name: build our own, and keep only a safe extension.
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension.Length < 2 || extension.Length > 7 || !extension.Skip(1).All(char.IsLetterOrDigit))
            {
                extension = ".audio";
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var folder = Path.Combine(_env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{subfolder}/{fileName}";
        }

        public void Delete(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = Path.GetFullPath(Path.Combine(
                _env.WebRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads")) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) return;

            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}
