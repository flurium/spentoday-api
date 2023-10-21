namespace Data.Models.ShopTables;

public class InfoPage
{
    public string Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string ShopId { get; set; }
    public Shop Shop { get; } = default!;

    public InfoPage(string slug, string shopId)
    {
        Slug = slug;
        ShopId = shopId;
    }
}

public static class InfoPageExtension
{
    public static IQueryable<InfoPage> WithDomain(this IQueryable<InfoPage> query, string Domain)
    {
        return query.Where(x => x.Shop.Domains.Any(x => x.Domain == Domain
            && x.Verified
        ));
    }
}