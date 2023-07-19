namespace Data.Models
{
    public class ShopBanner
    {
        public string Url { get; set; }
        public string ShopId { get; set; }
        public Shop Shop { get; set; }
    }
}