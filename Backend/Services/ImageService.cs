using Lib.Storage;
using Microsoft.AspNetCore.Mvc;

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
    private static readonly string[] photoExtensions = new string[] {
        ".xbm", ".tif", ".jfif", ".ico", ".tiff", ".gif", ".svg",".jpeg", ".svgz",
        ".jpg", ".webp", ".png", ".bmp", ".pjp", ".apng", ".pjpeg", ".avif"
    };

    public static bool IsImage(this IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName) || file.Length == 0) return false;

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        return photoExtensions.Contains(fileExtension);
    }
}

    public async Task SafeDeleteOne(IStorageFile file)
    {
            var deleted = await storage.Delete(file);
            if (deleted) return;

            background.Enqueue(async (provider) =>
            {
                using IServiceScope scope = provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IStorage>();
                await service.Delete(file);
            });
    }
    
    public bool IsImageFile(IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName) || file.Length == 0)
        {
            return false;
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        string[] photoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".tiff", ".ico", ".jfif", ".psd", ".eps", ".pict", ".pic", "pct" }; ;

        return photoExtensions.Contains(fileExtension);
    }
}