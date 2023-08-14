namespace Data.Models.ShopTables;

public class ShopDomain
{
    public string Domain { get; }

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public ShopDomain(string domain, string shopId)
    {
        Domain = domain;
        ShopId = shopId;
    }

    public bool IsOwned(string domain)
    {
        return Domain == domain;
    }
}