using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly Db db;
    private readonly ImageService imageService;

    public DashboardController(Db db, ImageService imageService)
    {
        this.db = db;
        this.imageService = imageService;
    }

    // TODO: refactor this bullshit
    [HttpDelete("delete/{shopId}")]
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

        foreach (var product in products)
        {
            if (product.Images != null) await imageService.SafeDelete(product.Images);
        }

        var shop = await db.Shops
            .Where(s => s.Id == shopId)
            .Include(s => s.Banners)
            .QueryOne();

        if (shop != null)
        {
            if (shop.Banners != null) await imageService.SafeDelete(shop.Banners);
            db.Shops.Remove(shop);
            var isSaved = await db.Save();
            if (!isSaved) return Problem();
        }

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    public record ShopOut(string Name, string Id);

    public record ShopAdd(string ShopName);

    [HttpPost("addshop")]
    [Authorize]
    public async Task<IActionResult> AddShop([FromBody] ShopAdd shop)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var newShop = new Shop(shop.ShopName, uid.Value);

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
}