namespace Data.Models;

public class Shop
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string LogoUrl { get; set; }

    public string OwnerId { get; set; }
    public User? Owner { get; set; }

    public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks { get; set; }
    public IReadOnlyCollection<Product> Products { get; set; }
    public IReadOnlyCollection<ShopBanner> Banners { get; set; }

    public Shop(string name, string logoUrl)
    {
        Name = name;
        LogoUrl = logoUrl;
    }
}