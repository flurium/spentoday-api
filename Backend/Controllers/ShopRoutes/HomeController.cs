using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly Db db;
    private readonly IStorage storage;

    public HomeController(Db db, IStorage storage)
    {
        this.db = db;
        this.storage = storage;
    }

    public record HomeCategory(string Id, string Name);
    public record HomeProduct(string Id, string Slug, string Name, double Price, double DiscountPrice, bool IsDiscount, StorageFile? Image);
    public record HomeBanner(string Id, string Url);
    public record HomeShop(string Id, string Name, string? TopBanner,
        IEnumerable<string> Slogans,
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

        var slogans = shop.Slogan.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

        var products = await db.Products
            .Where(x => x.ShopId == shop.Id)
            .Select(p => new HomeProduct(
                p.Id,
                p.SeoSlug,
                p.Name,
                p.Price,
                p.DiscountPrice,
                p.IsDiscount,
              p.Images.OrderByDescending(x => x.Id == p.PreviewImage).Select(x => x.GetStorageFile()).FirstOrDefault()
            ))
            .Take(4).QueryMany();

        var layoutShop = new HomeShop(shop.Id, shop.Name, topBannerUrl, slogans, categories, banners, products);

        return Ok(layoutShop);
    }
}