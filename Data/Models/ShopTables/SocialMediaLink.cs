namespace Data.Models.ShopTables;

public class SocialMediaLink
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Link { get; set; }

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public SocialMediaLink(string name, string link, string shopId)
    {
        Name = name;
        Link = link;
        ShopId = shopId;
    }
}