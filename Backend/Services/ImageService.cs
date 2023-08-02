using Lib.Storage;

namespace Backend.Services;

/// <summary>
/// Help to delete list of files from storage in safe way.
/// By using background runner.
/// </summary>
public class ImageService
{
    private readonly IStorage storage;
    private readonly BackgroundQueue background;

    public ImageService(IStorage storage, BackgroundQueue background)
    {
        this.storage = storage;
        this.background = background;
    }

    /// <summary>
    /// Safely delete list of files.
    /// If some file isn't deleted it adds retry of deletion in background process.
    /// </summary>
    public async Task SafeDelete(IEnumerable<IStorageFile> files)
    {
        foreach (var file in files)
        {
            var deleted = await storage.Delete(file);
            if (deleted) continue;

            background.Enqueue(async (provider) =>
            {
                using IServiceScope scope = provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IStorage>();
                await service.Delete(file);
            });
        }
    }
}

public static class ImageExtension
{
    public static bool IsImage(this IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName) || file.Length == 0) return false;

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        string[] photoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".tiff", ".ico", }; ;
        return photoExtensions.Contains(fileExtension);
    }
}