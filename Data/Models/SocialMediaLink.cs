namespace Data.Models
{
    public class SocialMediaLink
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string ShopId { get; set; }
        public Shop Shop { get; set; }
    }
}