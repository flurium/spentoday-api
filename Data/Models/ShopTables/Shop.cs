using Data.Models.ProductTables;
using Data.Models.UserTables;

namespace Data.Models.ShopTables;

public class Shop
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string LogoUrl { get; set; }

    public string OwnerId { get; set; }
    public User Owner { get; } = default!;

    public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks { get; } = default!;
    public IReadOnlyCollection<Product> Products { get; } = default!;
    public IReadOnlyCollection<ShopBanner> Banners { get; } = default!;
    public IReadOnlyCollection<InfoPage> InfoPages { get; } = default!;

    public Shop(string name, string logoUrl, string ownerId)
    {
        Name = name;
        LogoUrl = logoUrl;
        OwnerId = ownerId;
    }
}