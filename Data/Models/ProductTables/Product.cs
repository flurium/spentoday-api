using Data.Models.ShopTables;

namespace Data.Models.ProductTables;

public class Product
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public double Price { get; set; } = 0;
    public int Amount { get; set; } = 0;
    public string PreviewImage { get; set; }

    public IReadOnlyCollection<Order> Orders { get; set; }

    public string ShopId { get; set; }
    public Shop Shop { get; set; }
    public bool IsDraft { get; set; } = true;
    public bool IsArchive { get; set; } = false;
    public string? VideoUrl { get; set; }

    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoSlug { get; set; } = string.Empty;

    public IReadOnlyCollection<ProductImage> Images { get; set; }
    public IReadOnlyCollection<ProductCategory> ProductCategories { get; set; }

    public string ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public Product(string name, double price, int amount, string previewImage, string shopId, string? videoUrl = null)
    {
        Name = name;
        Price = price;
        Amount = amount;
        PreviewImage = previewImage;
        ShopId = shopId;
        VideoUrl = videoUrl;
    }
}