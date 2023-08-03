using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Lib;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly Db db;
    private readonly ImageService imageService;
    private readonly IStorage storage;

    public ProductController(Db db, ImageService imageService, IStorage storage)
    {
        this.db = db;
        this.imageService = imageService;
        this.storage = storage;
    }

    public record ListOutput(string Id, string Name, double Price, bool IsDraft);

    [HttpGet("shop/{shopId}"), Authorize]
    public async Task<IActionResult> List(
        [FromRoute] string shopId, [FromQuery] int start = 0, [FromQuery] int count = 10
    )
    {
        var uid = User.Uid();
        var products = await db.Products
                .Skip(start).Take(count)
                .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
                .Select(x => new ListOutput(x.Id, x.Name, x.Price, x.IsDraft))
                .QueryMany();

        return Ok(products);
    }

    public record CreateInput(string Name, string SeoSlug, string ShopId);

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateInput input)
    {
        if (!input.SeoSlug.IsSlug()) return BadRequest();

        var uid = User.Uid();
        var ownShop = await db.Shops.Have(x => x.Id == input.ShopId && x.OwnerId == uid);
        if (!ownShop) return Forbid();

        var product = new Product(input.Name, input.SeoSlug, input.ShopId);
        await db.Products.AddAsync(product);

        var saved = await db.Save();
        return saved ? Ok(new ListOutput(product.Id, product.Name, product.Price, product.IsDraft)) : Problem();
    }

    public record ImageOutput(string Id, string Key, string Bucket, string Provider);
    public record ProductOutput(
        string Id, string Name, double Price, int Amount, bool IsDraft,
        string SeoTitle, string SeoDescription, string SeoSlug,
        string Description, IEnumerable<ImageOutput> Images
    );
    public record CategoryOutput(string Id, string Name, bool Assigned);
    public record OneOutput(ProductOutput Product, IEnumerable<CategoryOutput> Categories);

    [HttpGet("{id}"), Authorize]
    public async Task<IActionResult> One(string id)
    {
        var uid = User.Uid();
        var product = await db.Products
            .Where(x => x.Id == id && x.Shop.OwnerId == uid)
            .Select(x => new
            {
                Product = new ProductOutput(
                    x.Id, x.Name, x.Price, x.Amount, x.IsDraft,
                    x.SeoTitle, x.SeoDescription, x.SeoSlug, x.Description,
                    x.Images.Select(i => new ImageOutput(i.Id, i.Key, i.Bucket, i.Provider))
                ),
                ShopId = x.ShopId
            })
            .QueryOne();
        if (product == null) return NotFound();

        var categories = await db.Categories
            .Where(x => x.ShopId == product.ShopId)
            .Select(x => new CategoryOutput(
                x.Id, x.Name,
                x.ProductCategories.Any(x => x.ProductId == product.Product.Id)
            ))
            .QueryMany();

        var output = new OneOutput(product.Product, categories);
        return Ok(output);
    }

    public record class UpdateInput(
        string Id, string? Name, double? Price, int? Amount, string? Description,
        string? PreviewImage, string? SeoTitle, string? SeoDescription, string? SeoSlug
    );

    [HttpPatch, Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateInput input)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(p => p.Id == input.Id && p.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        if (input.Name != null) product.Name = input.Name;
        if (input.Description != null) product.Description = input.Description;
        if (input.Amount.HasValue) product.Amount = input.Amount.Value;
        if (input.Price.HasValue) product.Price = input.Price.Value;
        if (input.PreviewImage != null) product.PreviewImage = input.PreviewImage;
        if (input.SeoTitle != null) product.SeoTitle = input.SeoTitle;
        if (input.SeoSlug != null) product.SeoSlug = input.SeoSlug;
        if (input.SeoDescription != null) product.SeoDescription = input.SeoDescription;

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpPut("{id}/publish"), Authorize]
    public async Task<IActionResult> Publish([FromRoute] string id)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(x => x.Id == id && x.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        product.IsDraft = false;
        var saved = await db.Save();

        return saved ? Ok() : Problem();
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(p => p.Id == id && p.Shop.OwnerId == uid);

        if (product == null) return Problem();

        bool canDeleteProduct = !await db.Orders.AnyAsync(o => o.ProductId == id);

        if (canDeleteProduct)
        {
            var images = await db.ProductImages.QueryMany(x => x.ProductId == product.Id);
            await imageService.SafeDelete(images);
            db.Products.Remove(product);
        }
        else
        {
            // product.IsArchive = true;
        }

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    // IMAGES

    [HttpPost("{id}/image"), Authorize]
    public async Task<IActionResult> UploadProductImage(IFormFile file, [FromRoute] string id)
    {
        if (!file.IsImage()) return BadRequest();

        var uid = User.Uid();
        var ownProduct = await db.Products.Have(x => x.Id == id && x.Shop.OwnerId == uid);
        if (!ownProduct) return NotFound();

        var imageCount = await db.ProductImages.CountAsync(x => x.ProductId == id).ConfigureAwait(false);
        if (imageCount >= 12) return Conflict();

        var fileId = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadedFile = await storage.Upload(fileId, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        var image = new ProductImage(uploadedFile.Provider, uploadedFile.Bucket, uploadedFile.Key, id);
        await db.ProductImages.AddAsync(image);
        var saved = await db.Save();

        if (saved) return Ok(new ImageOutput(image.Id, image.Key, image.Bucket, image.Provider));

        await storage.Delete(image);
        return Problem();
    }

    [HttpDelete("image/{id}"), Authorize]
    public async Task<IActionResult> DeleteProductImage([FromRoute] string id)
    {
        var uid = User.Uid();
        var image = await db.ProductImages.QueryOne(pi => pi.Id == id && pi.Product.Shop.OwnerId == uid);
        if (image == null) return NotFound();

        bool isDeleted = await storage.Delete(image);
        if (!isDeleted) return Problem();

        db.ProductImages.Remove(image);
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }
}