using Data.Models.ShopTables;
using Lib.Storage;
using Microsoft.AspNetCore.Identity;

namespace Data.Models.UserTables;

public class User : IdentityUser, IStorageFileContainer
{
    public int Version { get; set; } = 0;

    public IReadOnlyCollection<Shop> Shops { get; set; } = default!;

    public string Name { get; set; }

    public UserImage? Image { get; set; }

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
        if (ImageKey == null) return null;
        if (ImageBucket == null) return null;
        if (ImageProvider == null) return null;
        return new StorageFile(ImageBucket, ImageKey, ImageProvider);
    }
}