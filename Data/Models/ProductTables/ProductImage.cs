using Lib.Storage;

namespace Data.Models.ProductTables;

public class ProductImage : IStorageFile
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public string Provider { get; set; }
    public string Bucket { get; set; }
    public string Key { get; set; }

    public string ProductId { get; set; }
    public Product? Product { get; set; }

    public ProductImage(string provider, string bucket, string key, string productId)
    {
        Provider = provider;
        Bucket = bucket;
        Key = key;
        ProductId = productId;
    }
}