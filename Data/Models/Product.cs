namespace Data.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public double Price { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string PreviewImage { get; set; }
        public List<ProductImage> Images { get; set; }

        //public List<CategoryToProduct> Categories?
        public string ShopId { get; set; }

        public Shop Shop { get; set; }

        public string? VideoUrl { get; set; }

        //SEO
        public string SeoTitle { get; set; } = string.Empty;

        public string SeoDescription { get; set; } = string.Empty;
        public string SeoSlug { get; set; } = string.Empty;
    }
}