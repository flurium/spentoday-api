namespace Data.Models
{
    public class Shop
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public List<SocialMediaLink> SocialMediaLinks { get; set; }
        public List<Product> Products { get; set; }
        public List<ShopBanner> Banners { get; set; }
    }
}