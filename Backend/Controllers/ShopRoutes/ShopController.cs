using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class ShopController : ControllerBase
{
    private readonly Db db;
    private readonly IStorage storage;

    public ShopController(Db db, IStorage storage)
    {
        this.db = db;
        this.storage = storage;
    }

    public record HomeCategory(string Id, string Name);
    public record HomeProduct(string Id, string Name, string Price, string? Image);
    public record HomeBanner(string Id, string Url);
    public record HomeShop(string Id, string Name, string? TopBanner,
        IEnumerable<HomeCategory> Categories,
        IEnumerable<HomeBanner> Banners,
        IEnumerable<HomeProduct> Products
    );

    [HttpGet("home/{shopDomain}")]
    public async Task<IActionResult> Home([FromRoute] string shopDomain)
    {
        var shop = await db.Shops.WithDomain(shopDomain).QueryOne();
        if (shop == null) return NotFound();

        var categories = await db.Categories
            .Where(x => x.ShopId == shop.Id && x.ParentId == null)
            .Select(x => new HomeCategory(x.Id, x.Name))
            .QueryMany();

        var banners = await db.ShopBanners
            .Where(x => x.ShopId == shop.Id)
            .Select(x => new HomeBanner(x.Id, storage.Url(x.GetStorageFile())))
            .QueryMany();

        var topBannerIndex = banners.FindIndex(x => x.Id == shop.TopBannerId);
        string? topBannerUrl;
        if (topBannerIndex < 0) { topBannerUrl = null; }
        else
        {
            topBannerUrl = banners[topBannerIndex].Url;
            banners.RemoveAt(topBannerIndex);
        }

        var products = await db.Products
            .Where(x => x.ShopId == shop.Id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Price = p.Price.ToString("F2"),
                Image = p.Images.OrderByDescending(x => x.Id == p.PreviewImage).FirstOrDefault()
            })
            .Take(4).QueryMany();

        var homeProducts = products
            .Select(x =>
            {
                string? url = x.Image != null ? storage.Url(x.Image.GetStorageFile()) : null;
                return new HomeProduct(x.Id, x.Name, x.Price, url);
            });

        var layoutShop = new HomeShop(shop.Id, shop.Name, topBannerUrl, categories, banners, homeProducts);

        return Ok(layoutShop);
    }
}