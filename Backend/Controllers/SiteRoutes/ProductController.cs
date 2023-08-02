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

    public record OneOutput(
        string Id, string Name, double Price, int Amount, bool IsDraft,
        string SeoTitle, string SeoDescription, string SeoSlug,
        IEnumerable<ImageOutput> Images
    );

    public record ImageOutput(string Id, string Key, string Bucket, string Provider);

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<Product>> One(string id)
    {
        var uid = User.Uid();
        var product = await db.Products
            .Where(x => x.Id == id && x.Shop.OwnerId == uid)
            .Select(x => new OneOutput(
                x.Id, x.Name, x.Price, x.Amount, x.IsDraft,
                x.SeoTitle, x.SeoDescription, x.SeoSlug,
                x.Images.Select(i => new ImageOutput(i.Id, i.Key, i.Bucket, i.Provider))
            ))
            .QueryOne();

        if (product == null) return NotFound();

        return Ok(product);
    }

    public record class UpdateInput(
        string Id, string? Name, double? Price, int? Amount,
        string? PreviewImage, string? SeoTitle, string? SeoDescription, string? SeoSlug
    );

    [HttpPatch, Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateInput patchDoc)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(p => p.Id == patchDoc.Id && p.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        if (patchDoc.Name != null) product.Name = patchDoc.Name;
        if (patchDoc.Amount.HasValue) product.Amount = patchDoc.Amount.Value;
        if (patchDoc.Price.HasValue) product.Price = patchDoc.Price.Value;
        if (patchDoc.PreviewImage != null) product.PreviewImage = patchDoc.PreviewImage;
        if (patchDoc.SeoTitle != null) product.SeoTitle = patchDoc.SeoTitle;
        if (patchDoc.SeoSlug != null) product.SeoSlug = patchDoc.SeoSlug;
        if (patchDoc.SeoDescription != null) product.SeoDescription = patchDoc.SeoDescription;

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
        var uid = User.Uid();
        var product = await db.Products.QueryOne(x => x.Id == id && x.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        if (!file.IsImage()) return BadRequest();

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