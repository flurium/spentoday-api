using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;

using Lib.Storage;
using Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Controllers.Dashboard;

[Route("v1/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IStorage storage;
    private readonly Db db;
    private readonly ImageService imageService;
    private readonly BackgroundQueue background;

    public DashboardController(IStorage storage, Db db, ImageService imageService, BackgroundQueue background)
    {
        this.storage = storage;
        this.db = db;
        this.imageService = imageService;
        this.background = background;
    }

    [HttpDelete("{shopId}")]
    [Authorize]
    public async Task<IActionResult> DeleteShop([FromRoute] string shopId)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var products = await db.Products
            .Where(x => x.ShopId == shopId)
            .Include(p => p.Images)
            .Include(p => p.ProductCategories)
            .QueryMany();
        if (products != null)
        {
            foreach (var product in products)
            {
                await imageService.SafeDelete(product.Images);

                db.Products.Remove(product);

                var isSaved = await db.Save();
                if (!isSaved) return Problem();
            }
        }

        var shop = await db.Shops
            .Where(s => s.Id == shopId)
            .Include(s => s.Banners)
            .Include(s => s.InfoPages)
            .Include(s => s.SocialMediaLinks)
            .QueryOne();

        if (shop != null)
        {
            await imageService.SafeDelete(shop.Banners);
            db.Shops.Remove(shop);
            var isSaved = await db.Save();
            if (!isSaved) return Problem();
        }

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    public record ShopOut(string Name, string Id);

    public record ShopAdd(string shopName);

    [HttpPost("addshop")]
    [Authorize]
    public async Task<IActionResult> AddShop([FromBody] ShopAdd shop)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        //var key = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        //var upload = await storage.Upload(key, file.OpenReadStream());
        //if (upload == null) return Problem();

        //var link = storage.Url(upload);
        //if (link == null) return Problem();

        var newShop = new Shop(shop.shopName, uid.Value);

        await db.Shops.AddAsync(newShop);

        var saved = await db.Save();
        return saved ? Ok(new ShopOut(newShop.Name, newShop.Id)) : Problem();
    }

    [HttpGet("shops")]
    [Authorize]
    public async Task<IActionResult> Shops()
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var shops = await db.Shops
            .Where(x => x.OwnerId == uid.Value)
            .Select(x => new ShopOut(x.Name, x.Id))
            .QueryMany();

        return Ok(shops);
    }

    /*
    public record ShopFilter(string shopName);
    [HttpGet("shops/filter")]
    [Authorize]
    public async Task<IActionResult> FilterShops([FromBody] ShopFilter search)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var shops = await db.Shops
            .Where(x => x.OwnerId == uid.Value && x.Name.Contains(search.shopName))
            .QueryMany();

        return Ok(shops);
    }*/
}