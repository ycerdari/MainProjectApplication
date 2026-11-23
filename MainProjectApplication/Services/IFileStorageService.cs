namespace PUPApp.Services
{
    // Abstraction (DIP): controllers depend on this interface, not on how files are stored.
    // Today it saves to wwwroot/uploads; later it could be swapped for cloud storage
    // without touching any controller.
    public interface IFileStorageService
    {
        // Saves the file and returns its web path, e.g. "/uploads/audio/3f2a...mp3"
        Task<string> SaveAsync(IFormFile file, string subfolder);

        // Deletes a previously saved file (safe to call with null or a missing file)
        void Delete(string? relativePath);
    }
}
