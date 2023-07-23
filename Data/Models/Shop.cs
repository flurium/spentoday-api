namespace Data.Models;

public class Shop
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string LogoUrl { get; set; }

    public string OwnerId { get; set; }
    public User Owner { get; set; } = default!;

    public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks { get; set; } = default!;
    public IReadOnlyCollection<Product> Products { get; set; } = default!;
    public IReadOnlyCollection<ShopBanner> Banners { get; set; } = default!;

    public Shop(string name, string logoUrl, string ownerId)
    {
        Name = name;
        LogoUrl = logoUrl;
        OwnerId = ownerId;
    }
}