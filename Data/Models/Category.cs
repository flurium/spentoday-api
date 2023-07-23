namespace Data.Models;

public class Category
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public IReadOnlyCollection<ProductCategory> ProductCategories { get; set; } = default!;

    public Category(string name)
    {
        Name = name;
    }
}