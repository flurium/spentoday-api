using Data;
using Data.Models.ShopTables;
using Lib;
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
    public record HomeProduct(string Id, string Name, string Price, string Image);
    public record HomeBanner(string Id, string Url);
    public record HomeShop(string Id, string Name, string TopBanner,
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
          .Where(x => x.ShopId == shop.Id && x.Id != shop.TopBannerId)
          .Select(x => new HomeBanner(x.Id, storage.Url(x.GetStorageFile())))
          .QueryMany();

        var topBanner = await db.ShopBanners.QueryOne(x => x.Id == shop.TopBannerId);
        StorageFile? topBannerFile = topBanner?.GetStorageFile();

        string? top = topBanner == null ? null : storage.Url(topBannerFile);

        var products = await db.Products.Where(x => x.ShopId == shop.Id).Select(p => new HomeProduct(
                p.Id, p.Name, p.Price.ToString("F2"),
                p.Images.FirstOrDefault(x => x.Id == p.PreviewImage) == null
                ? p.Images.FirstOrDefault() != null ? storage.Url(p.Images.FirstOrDefault().GetStorageFile())
                : "" : storage.Url(p.Images.FirstOrDefault(x => x.Id == p.PreviewImage).GetStorageFile())
            ))
            .Take(4).QueryMany();

        var layoutShop = new HomeShop(shop.Id, shop.Name, top, categories, banners, products);

        return Ok(layoutShop);
    }
}