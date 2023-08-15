using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class ShopController : ControllerBase
{
    private readonly Db db;

    public ShopController(Db db)
    {
        this.db = db;
    }

    public record struct HomeSocialMediaLink(string Name, string Link);
    public record struct HomeBanner(string Key, string Bucket, string Provider);
    public record HomeShop(string Id, string Name, IEnumerable<HomeSocialMediaLink> SocialMediaLinks, IEnumerable<HomeBanner> Banners);

    [HttpGet("home/{shopDomain}")]
    public async Task<IActionResult> Home([FromRoute] string shopDomain)
    {
        var shop = await db.Shops
            .Where(x => x.Domains.Any(x => x.Domain == shopDomain))
            .Include(x => x.Banners)
            .Include(x => x.SocialMediaLinks)
            .Select(x => new HomeShop(
                x.Id,
                x.Name,
                x.SocialMediaLinks.Select(l => new HomeSocialMediaLink(l.Name, l.Link)),
                x.Banners.Select(b => new HomeBanner(b.Key, b.Bucket, b.Provider))
            ))
            .QueryOne();

        if (shop == null) return NotFound();
        return Ok(shop);
    }
}