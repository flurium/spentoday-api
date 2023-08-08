namespace Data.Models.ProductTables;

public class ProductCategory
{
    public string ProductId { get; set; }
    public Product Product { get; } = default!;

    public string CategoryId { get; set; }
    public Category Category { get; } = default!;

    public int Order { get; set; }

    public ProductCategory(string productId, string categoryId, int order)
    {
        ProductId = productId;
        CategoryId = categoryId;
        Order = order;
    }
}