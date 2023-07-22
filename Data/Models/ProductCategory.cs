namespace Data.Models;

public class ProductCategory
{
    public string ProductId { get; set; }
    public Product? Product { get; set; }

    public string CategoryId { get; set; }
    public Category? Category { get; set; }

    public ProductCategory(string productId, string categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}