using Data.Models.ShopTables;

namespace Data.Models.ProductTables;

public class Product
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public double Price { get; set; } = 0;
    public double DiscountPrice { get; set; } = 0;
    public bool IsDiscount { get; set; } = false;
    public int Amount { get; set; } = 0;
    public string? PreviewImage { get; set; }

    public string Description { get; set; } = string.Empty;

    //  public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsDraft { get; set; } = true;

    public string SeoTitle { get; set; } = string.Empty;
    public string SeoDescription { get; set; } = string.Empty;
    public string SeoSlug { get; set; } = string.Empty;

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public IReadOnlyCollection<ProductImage> Images { get; } = default!;
    public List<ProductCategory> ProductCategories { get; set; } = default!;
    public IReadOnlyCollection<OrderProduct> OrderProducts { get; } = default!;

    public Product(string name, string seoSlug, string shopId)
    {
        Name = name;
        ShopId = shopId;
        SeoSlug = seoSlug;
    }
}

public static class ProductExtension
{
    public static IQueryable<Product> WithDomain(this IQueryable<Product> query, string Domain)
    {
        return query.Where(x => x.Shop.Domains.Any(x => x.Domain == Domain
            && x.Verified
        ));
    }
}