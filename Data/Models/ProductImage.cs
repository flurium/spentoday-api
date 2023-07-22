using Lib.Storage;

namespace Data.Models;

public class ProductImage : IStorageFile
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public string Provider { get; }
    public string Bucket { get; }
    public string Key { get; }

    public string ProductId { get; set; }
    public Product? Product { get; set; }

    public ProductImage(IStorageFile file, string productId)
    {
        Provider = file.Provider;
        Bucket = file.Bucket;
        Key = file.Key;
        ProductId = productId;
    }
}