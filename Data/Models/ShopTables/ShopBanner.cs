using Lib.Storage;

namespace Data.Models.ShopTables;

public class ShopBanner : IStorageFile, IStorageFileContainer
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public string ShopId { get; }
    public Shop Shop { get; } = default!;

    public string Provider { get; set; }
    public string Bucket { get; set; }
    public string Key { get; set; }

    public ShopBanner(string provider, string bucket, string key, string shopId)
    {
        Provider = provider;
        Bucket = bucket;
        Key = key;
        ShopId = shopId;
    }

    public StorageFile? GetStorageFile()
    {
        return new StorageFile(Bucket, Key, Provider);
    }
}