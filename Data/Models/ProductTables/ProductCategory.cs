namespace Data.Models.ProductTables;

public class ProductCategory
{
    public string ProductId { get; set; }
    public Product Product { get; } = default!;

    public string CategoryId { get; set; }
    public Category Category { get; } = default!;

    public ProductCategory(string productId, string categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}