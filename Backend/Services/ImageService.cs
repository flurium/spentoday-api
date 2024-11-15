﻿using Lib;
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
    public async Task SafeDelete(IEnumerable<IStorageFileContainer> files)
    {
        foreach (var file in files)
        {
            var deleted = await storage.Delete(file.GetStorageFile());
            if (deleted) continue;

            await background.Enqueue(async (provider) =>
            {
                using IServiceScope scope = provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IStorage>();
                await service.Delete(file.GetStorageFile());
            });
        }
    }

    public async Task SafeDelete(StorageFile file)
    {
        var deleted = await storage.Delete(file);
        if (deleted) return;

        await background.Enqueue(async (provider) =>
        {
            using IServiceScope scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IStorage>();
            await service.Delete(file);
        });
    }

    public async Task SafeDelete(IStorageFileContainer file) => await SafeDelete(file.GetStorageFile());
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