using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/shopsettings")]
[ApiController]
public class ShopSettingsController : ControllerBase
{
    private readonly Db db;
    private readonly ImageService imageService;

    private readonly IStorage storage;

    public ShopSettingsController(Db context, ImageService imageService, IStorage storage)
    {
        db = context;
        this.imageService = imageService;
        this.storage = storage;
    }

    public record LinkIn(string Name, string Link);
    public record LinkOut(string Name, string Link, string Id);
    public record BannerOut(string Url, string Id);
    public record ShopUpdate(string Name);

    [HttpPost("{shopId}/link")]
    [Authorize]
    public async Task<IActionResult> AddLink([FromBody] LinkIn link, [FromRoute] string shopId)
    {
        var uid = User.Uid();

        var shop = await db.Shops.QueryOne(x => x.Id == shopId && x.OwnerId == uid);
        if (shop == null) return Problem();

        var newLink = new SocialMediaLink(link.Name, link.Link, shopId);
        await db.SocialMediaLinks.AddAsync(newLink);

        var saved = await db.Save();
        return saved ? Ok(new LinkOut(newLink.Name, newLink.Link, newLink.Id)) : Problem();
    }

    [HttpDelete("link/{linkId}")]
    [Authorize]
    public async Task<IActionResult> DeleteLink([FromRoute] string linkId)
    {
        var uid = User.Uid();

        var link = await db.SocialMediaLinks.QueryOne(x => x.Id == linkId);
        if (link == null) return NotFound();

        db.SocialMediaLinks.Remove(link);

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpPost("{shopId}/banner")]
    [Authorize]
    public async Task<IActionResult> AddBanner(IFormFile file, [FromRoute] string shopId)
    {
        var uid = User.Uid();

        if (!file.IsImage()) return BadRequest();

        var shopOwned = await db.Shops.Have(x => x.Id == shopId && x.OwnerId == uid);
        if (!shopOwned) return Problem();

        var fileKey = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        var uploadedFile = await storage.Upload(fileKey, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        var shopBanner = new ShopBanner(uploadedFile.Provider, uploadedFile.Bucket, uploadedFile.Key, shopId);
        await db.ShopBanners.AddAsync(shopBanner);
        var saved = await db.Save();

        if (!saved)
        {
            await imageService.SafeDelete(uploadedFile);
            return Problem();
        }

        return Ok(new BannerOut(storage.Url(uploadedFile), shopBanner.Id));
    }

    [HttpDelete("banner/{bannerId}")]
    [Authorize]
    public async Task<IActionResult> DeleteBanner([FromRoute] string bannerId)
    {
        var uid = User.Uid();

        var banner = await db.ShopBanners.QueryOne(x => x.Id == bannerId);
        if (banner == null) return Problem();

        db.ShopBanners.Remove(banner);
        var saved = await db.Save();
        if (!saved) return Problem();

        await imageService.SafeDelete(banner);
        return Ok();
    }

    [HttpPost("{shopId}/name")]
    [Authorize]
    public async Task<IActionResult> UpdateShopName([FromRoute] string shopId, [FromBody] ShopUpdate shopName)
    {
        var uid = User.Uid();

        var shop = await db.Shops.QueryOne(x => x.Id == shopId && x.OwnerId == uid);
        if (shop == null) return NotFound();

        shop.Name = shopName.Name;

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpPost("{shopId}/logo")]
    [Authorize]
    public async Task<IActionResult> UploadLogo([FromRoute] string shopId, IFormFile file)
    {
        var uid = User.Uid();

        var shop = await db.Shops.QueryOne(x => x.Id == shopId && x.OwnerId == uid);
        if (shop == null) return Problem();

        if (!file.IsImage()) return BadRequest();

        var previousLogo = shop.GetStorageFile();

        var fileId = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var uploadedFile = await storage.Upload(fileId, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        shop.LogoBucket = uploadedFile.Bucket;
        shop.LogoKey = uploadedFile.Key;
        shop.LogoProvider = uploadedFile.Provider;

        var saved = await db.Save();
        if (!saved)
        {
            await imageService.SafeDelete(uploadedFile);
            return Problem();
        }

        if (previousLogo != null) await imageService.SafeDelete(previousLogo);
        return Ok(storage.Url(uploadedFile));
    }

    [NonAction]
    private async Task<ShopBanner?> ShopTopBanner(string? bannerId, string shopId)
    {
        if (bannerId == null) return null;
        var banner = await db.ShopBanners.QueryOne(x => x.Id == bannerId && x.ShopId == shopId);
        return banner;
    }

    [HttpPost("{shopId}/top")]
    [Authorize]
    public async Task<IActionResult> UploadTopBanner([FromRoute] string shopId, IFormFile file)
    {
        var uid = User.Uid();

        var shop = await db.Shops.QueryOne(x => x.Id == shopId && x.OwnerId == uid);
        if (shop == null) return Problem();

        if (!file.IsImage()) return BadRequest();
        var fileId = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        var uploadedFile = await storage.Upload(fileId, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        StorageFile? toDelete = null;

        var currentTopBanner = await ShopTopBanner(shop.TopBannerId, shop.Id);
        if (currentTopBanner == null)
        {
            var newTopBanner = new ShopBanner(uploadedFile.Provider, uploadedFile.Bucket, uploadedFile.Key, shop.Id);
            await db.AddAsync(newTopBanner);
            shop.TopBannerId = newTopBanner.Id;
        }
        else
        {
            toDelete = currentTopBanner.GetStorageFile();

            currentTopBanner.Provider = uploadedFile.Provider;
            currentTopBanner.Bucket = uploadedFile.Bucket;
            currentTopBanner.Key = uploadedFile.Key;
        }

        var saved = await db.Save();
        if (!saved)
        {
            await imageService.SafeDelete(uploadedFile);
            return Problem();
        }

        if (toDelete != null) await imageService.SafeDelete(toDelete);

        return Ok(storage.Url(uploadedFile));
    }

    public record ShopOut(string Name, string Logo, string TopBanner, List<BannerOut> Banners, List<LinkOut> Links);

    [HttpGet("shop/{shopId}")]
    [Authorize]
    public async Task<IActionResult> GetShop([FromRoute] string shopId)
    {
        var uid = User.Uid();

        var shop = await db.Shops.QueryOne(x => x.Id == shopId && x.OwnerId == uid);
        if (shop == null) return NotFound();

        var banners = await db.ShopBanners
            .Where(x => x.ShopId == shop.Id && x.Id != shop.TopBannerId)
            .Select(x => new BannerOut(storage.Url(x.GetStorageFile()), x.Id))
            .QueryMany();

        var links = await db.SocialMediaLinks
           .Where(x => x.ShopId == shop.Id)
           .Select(x => new LinkOut(x.Name, x.Link, x.Id))
           .QueryMany();

        var logoFile = shop.GetStorageFile();
        string logo = logoFile == null
            ? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRDsRxTnsSBMmVvRxdygcb9ue6xfUYL58YX27JLNLohHQ&s"
            : storage.Url(logoFile);

        var topBanner = await db.ShopBanners.QueryOne(x => x.Id == shop.TopBannerId);
        StorageFile? topBannerFile = topBanner?.GetStorageFile();

        string top = topBannerFile == null
            ? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRDsRxTnsSBMmVvRxdygcb9ue6xfUYL58YX27JLNLohHQ&s"
            : storage.Url(topBannerFile);

        var shopOut = new ShopOut(shop.Name, logo, top, banners, links);

        var saved = await db.Save();
        return saved ? Ok(shopOut) : Problem();
    }
}