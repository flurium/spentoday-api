using Lib.Storage;

namespace Data.Models;

public class ShopBanner : IStorageFile
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public string ShopId { get; set; }
    public Shop? Shop { get; set; }

    public string Provider { get; }
    public string Bucket { get; }
    public string Key { get; }

    public ShopBanner(IStorageFile file, string shopId)
    {
        Provider = file.Provider;
        Bucket = file.Bucket;
        Key = file.Key;
        ShopId = shopId;
    }
}