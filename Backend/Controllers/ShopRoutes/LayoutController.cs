using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class LayoutController : ControllerBase
{
    private readonly Db db;

    public LayoutController(Db db)
    {
        this.db = db;
    }

    public record LayoutCategory(string Id, string Name);
    public record LayoutPage(string Slug, string Name);
    public record LayoutSocialMedia(string Name, string Link);
    public record LayoutShop(string Id, string Name,
        IEnumerable<LayoutCategory> Categories, IEnumerable<LayoutPage> Pages,
        IEnumerable<LayoutSocialMedia> SocialMediaLinks
    );

    [HttpGet("{domain}/layout")]
    public async Task<IActionResult> Layout([FromRoute] string domain)
    {
        var shop = await db.Shops
            .WithDomain(domain)
            .Include(x => x.Categories)
            .Include(x => x.InfoPages)
            .Select(x => new LayoutShop(
                x.Id, x.Name,
                x.Categories.Select(x => new LayoutCategory(x.Id, x.Name)),
                x.InfoPages.Select(x => new LayoutPage(x.Slug, x.Title)),
                x.SocialMediaLinks.Select(x => new LayoutSocialMedia(x.Name, x.Link))
            ))
            .QueryOne();
        if (shop == null) return NotFound();
        return Ok(shop);
    }
}