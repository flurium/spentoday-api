using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Backend.Controllers.ShopRoutes.ShopController;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class LayoutController : ControllerBase
{
    private readonly Db db;
    private readonly IStorage storage;

    public LayoutController(Db db, IStorage storage)
    {
        this.db = db;
        this.storage = storage;
    }

    public record LayoutCategory(string Id, string Name);
    public record LayoutProduct(string Id, string Name, string Price, string Image);
    public record LayoutPage(string Slug, string Name);
    public record LayoutSocialMedia(string Name, string Link);
    public record LayoutBanner(string Id, string Url);
    public record LayoutShop(string Id, string Name, string TopBanner,
        IEnumerable<LayoutCategory> Categories, IEnumerable<LayoutPage> Pages,
        IEnumerable<LayoutSocialMedia> SocialMediaLinks,
        IEnumerable<LayoutBanner> Banners,
        IEnumerable<LayoutProduct> Products
    );

    [HttpGet("{domain}/layout")]
    public async Task<IActionResult> Layout([FromRoute] string domain)
    {
        var shop = await db.Shops.WithDomain(domain).QueryOne();
        if (shop == null) return NotFound();

        var categories = await db.Categories.Where(x => x.ShopId == shop.Id && x.ParentId == null).Select(x => new LayoutCategory(x.Id, x.Name)).QueryMany();

        var infoPage = await db.InfoPages.Where(x => x.ShopId == shop.Id).Select(x => new LayoutPage(x.Slug, x.Title)).QueryMany();

        var socialMediaLinks = await db.SocialMediaLinks.Where(x => x.ShopId == shop.Id).Select(x => new LayoutSocialMedia(x.Name, x.Link)).QueryMany();

        var banners = await db.ShopBanners
          .Where(x => x.ShopId == shop.Id && x.Id != shop.TopBannerId)
         .Select(x => new LayoutBanner(x.Id, storage.Url(x.GetStorageFile())))
          .QueryMany();

        var topBanner = await db.ShopBanners.QueryOne(x => x.Id == shop.TopBannerId);
        StorageFile? topBannerFile = topBanner?.GetStorageFile();

        string top = topBannerFile == null
            ? "https://wotpack.ru/wp-content/uploads/2022/02/raspisanieban.jpg"
            : storage.Url(topBannerFile);

        var products = await db.Products.Where(x => x.ShopId == shop.Id).Select(p => new LayoutProduct(
                p.Id, p.Name, p.Price.ToString("F2"), p.Images.FirstOrDefault(x => x.Id == p.PreviewImage) == null ? p.Images.FirstOrDefault() == null ? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRDsRxTnsSBMmVvRxdygcb9ue6xfUYL58YX27JLNLohHQ&s" : storage.Url(p.Images.FirstOrDefault().GetStorageFile()) : storage.Url(p.Images.FirstOrDefault(x => x.Id == p.PreviewImage).GetStorageFile()))).QueryMany();

        var layoutShop = new LayoutShop(shop.Id, shop.Name, top, categories, infoPage, socialMediaLinks, banners, products);

        return Ok(layoutShop);
    }
}