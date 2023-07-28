using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public DashboardController( IStorage storage, Db db, ImageService imageService, BackgroundQueue background)
    {
      
        this.storage = storage;
        this.db = db;
        this.imageService = imageService;
        this.background = background;
    }

    public record PagesOutput(string Slug, string Title, DateTime UpdatedAt);

    [HttpGet("{shopId}/pages")]
    public async Task<IActionResult> Pages(string shopId)
    {
        var pages = await db.InfoPages
            .Where(x => x.ShopId == shopId)
            .Select(x => new PagesOutput(x.Slug, x.Title, x.UpdatedAt))
            .QueryMany();

        return Ok(pages);
    }

    [NonAction]
    public bool IsSlugValid(string slug)
    {
        slug = slug.ToLower().Trim('-');
        for (int i = 0; i < slug.Length; ++i)
        {
            char c = slug[i];

            if (!(char.IsLetter(c) || char.IsDigit(c) || c == '-'))
            {
                return false;
            }
        }
        return true;
    }

    public record NewPageInput(string Slug);

    /// <returns>
    /// 400 - slug is invalid
    /// 401 - unauthorized
    /// 404 - such show for this user isn't found
    /// 409 - slug is taken
    /// 500 - unexpected error or can't save in database
    /// 200 - created
    /// </returns>
    [HttpPost("{shopId}/page")]
    [Authorize]
    public async Task<IActionResult> NewPage([FromRoute] string shopId, [FromBody] NewPageInput input)
    {
        // validate slug
        var isValid = IsSlugValid(input.Slug);
        if (!isValid) return BadRequest();

        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var ownShop = await db.Shops.AnyAsync(x => x.OwnerId == uid.Value && x.Id == shopId).ConfigureAwait(false);
        if (!ownShop) return NotFound();

        var slugTaken = await db.InfoPages.AnyAsync(x => x.ShopId == shopId && x.Slug == input.Slug).ConfigureAwait(false);
        if (slugTaken) return Conflict();

        var newInfoPage = new InfoPage(input.Slug, shopId);
        await db.InfoPages.AddAsync(newInfoPage);
        var saved = await db.Save();

        return saved ? Ok(newInfoPage) : Problem();
    }

    public record UpdatePageInput(string? Slug, string? Title, string? Description, string? Content);

    /// <response code="401">User is unauthorized.</response>
    /// <response code="404">Page isn't found.</response>
    /// <response code="400">Slug isn't valid.</response>
    /// <response code="409">Slug alread exists.</response>
    /// <response code="500">Unexpected error or can't save to database.</response>
    /// <response code="200">Successfully updated.</response>
    [HttpPatch("{shopId}/page/{slug}")]
    [Authorize]
    public async Task<IActionResult> UpdatePage(
        [FromRoute] string shopId,
        [FromRoute] string slug,
        [FromBody] UpdatePageInput input
    )
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var page = await db.InfoPages
            .QueryOne(x => x.ShopId == shopId && x.Slug == slug && x.Shop.OwnerId == uid.Value);
        if (page == null) return NotFound();

        if (input.Slug != null)
        {
            var slugValid = IsSlugValid(input.Slug);
            if (!slugValid) return BadRequest();

            var slugTaken = await db.InfoPages.AnyAsync(x => x.ShopId == shopId && x.Slug == input.Slug).ConfigureAwait(false);
            if (slugTaken) return Conflict();

            page.Slug = input.Slug;
        }

        if (input.Title != null) page.Title = input.Title;
        if (input.Description != null) page.Description = input.Description;
        if (input.Content != null) page.Content = input.Content;

        var saved = await db.Save();
        return saved ? Ok(page) : Problem();
    }

    [HttpDelete("{shopId}")]
    [Authorize]
    public async Task<IActionResult> DeleteShop([FromRoute] string shopId)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var products = await db.Products
            .Where(x => x.ShopId == shopId)
            .Include(p=>p.Images)
            .Include(p=>p.ProductCategories)
            .QueryMany();
        if (products != null) {

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

    [HttpPut("{shopName}/shop")]
    [Authorize]
    public async Task<IActionResult> AddShop([FromRoute] string shopName, IFormFile file)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var key = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var upload = await storage.Upload(key, file.OpenReadStream());
        if (upload == null) return Problem();

        var link = storage.Url(upload);
        if (link == null) return Problem();

        var shop = new Shop(shopName, link, uid.Value);

        await db.Shops.AddAsync(shop);

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }



    [HttpGet("shops")]
    [Authorize]
    public async Task<IActionResult> Shops()
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var shops = await db.Shops
            .Where(x => x.OwnerId == uid.Value)
            .QueryMany();
        
        return Ok(shops);
    }

    /*[HttpGet("shops/filter/{search}")]
    [Authorize]
    public async Task<IActionResult> FilterShops([FromRoute] string search)
    {
        var uid = User.FindFirst(Jwt.Uid);
        if (uid == null) return Unauthorized();

        var shops = await db.Shops
            .Where(x => x.OwnerId == uid.Value && x.Name.Contains(search))
            .QueryMany();

        return Ok(shops);
    }*/
}