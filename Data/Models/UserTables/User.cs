using Data.Models.ShopTables;
using Lib.Storage;
using Microsoft.AspNetCore.Identity;

namespace Data.Models.UserTables;

public class User : IdentityUser, IStorageFileContainer
{
    public int Version { get; set; } = 0;
    public string Name { get; set; }

    public IReadOnlyCollection<Shop> Shops { get; set; } = default!;

    public User(string name, string email)
    {
        Name = name;
        Email = email;
        UserName = email;
    }

    public string? ImageProvider { get; set; }
    public string? ImageBucket { get; set; }
    public string? ImageKey { get; set; }

    public StorageFile? GetStorageFile()
    {
        if (ImageKey == null || ImageBucket == null || ImageProvider == null) return null;
        return new StorageFile(ImageBucket, ImageKey, ImageProvider);
    }
}