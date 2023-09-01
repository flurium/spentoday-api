namespace Data.Models.ShopTables;

public class Subscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; }

    public string ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public Subscription(string email, string shopId)
    {
        Email = email;
        ShopId = shopId;
    }
}