namespace Data.Models.ShopTables;

public class InfoPage
{
    public string Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public InfoPage(string slug, string shopId)
    {
        Slug = slug;
        ShopId = shopId;
    }
}