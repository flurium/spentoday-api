namespace Data.Models.ProductTables;

public class Property
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Key { get; set; }
    public string Value { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Property(string key, string value, string productId)
    {
        Key = key;
        Value = value;
        ProductId = productId;
    }
}