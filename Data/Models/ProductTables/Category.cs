using Data.Models.ShopTables;

namespace Data.Models.ProductTables;

public class Category
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public string? ParentId { get; set; }
    public Category? Parent { get; }

    public IReadOnlyCollection<Category> Subcategories { get; } = default!;
    public IReadOnlyCollection<ProductCategory> ProductCategories { get; } = default!;

    public Category(string name, string shopId, string? parentId = null)
    {
        Name = name;
        ShopId = shopId;
        ParentId = parentId;
    }
}