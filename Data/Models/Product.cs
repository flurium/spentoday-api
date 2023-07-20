namespace Data.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public double Price { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string PreviewImage { get; set; }
        public IReadOnlyCollection<ProductImage> Images { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }

        public string ShopId { get; set; }
        public Shop Shop { get; set; }
        public bool IsDraft { get; set; } = true;
        public string? VideoUrl { get; set; }

        //SEO
        public string SeoTitle { get; set; } = string.Empty;
        public string SeoDescription { get; set; } = string.Empty;
        public string SeoSlug { get; set; } = string.Empty;

        public Product(string Name, double Price, int Amount, string PreviewImage, string ShopId, string VideoUrl = null ) {
            this.Name = Name;
            this.Price = Price;
            this.Amount = Amount;
            this.PreviewImage = PreviewImage;
            this.ShopId = ShopId;
            this.VideoUrl = VideoUrl;
        }
    }
}