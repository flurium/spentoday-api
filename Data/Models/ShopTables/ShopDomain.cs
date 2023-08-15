namespace Data.Models.ShopTables;

public class ShopDomain
{
    public string Domain { get; }

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public bool Verified { get; set; } = false;

    public ShopDomain(string domain, string shopId, bool verified)
    {
        Domain = domain;
        ShopId = shopId;
        Verified = verified;
    }

    public bool IsOwned(string domain)
    {
        return Domain == domain;
    }
}