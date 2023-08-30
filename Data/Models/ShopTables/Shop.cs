using Data.Models.ProductTables;
using Data.Models.UserTables;
using Lib.Storage;

namespace Data.Models.ShopTables;

public class Shop : IPossibleStorageFileContainer
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }

    public string OwnerId { get; set; }
    public User Owner { get; } = default!;

    public IReadOnlyCollection<ShopDomain> Domains { get; } = default!;
    public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks { get; } = default!;
    public IReadOnlyCollection<Product> Products { get; } = default!;
    public IReadOnlyCollection<ShopBanner> Banners { get; } = default!;
    public IReadOnlyCollection<InfoPage> InfoPages { get; } = default!;
    public IReadOnlyCollection<Category> Categories { get; } = default!;

    public Shop(string name, string ownerId)
    {
        Name = name;
        OwnerId = ownerId;
    }

    public string? LogoProvider { get; set; }
    public string? LogoBucket { get; set; }
    public string? LogoKey { get; set; }

    public StorageFile? GetStorageFile()
    {
        if (LogoKey == null || LogoBucket == null || LogoProvider == null) return null;
        return new StorageFile(LogoBucket, LogoKey, LogoProvider);
    }

    public string? TopBannerId { get; set; }
}

public static class ShopExtension
{
    public static IQueryable<Shop> WithDomain(this IQueryable<Shop> query, string Domain)
    {
        return query.Where(x => x.Domains.Any(x => x.Domain == Domain
        //&& x.Verified
        ));
    }
}