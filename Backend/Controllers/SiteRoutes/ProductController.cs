using Backend.Auth;
using Backend.Features.Categories;
using Backend.Services;
using Data;
using Data.Models.ProductTables;
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
    private readonly CategoryService categoryService;

    public ProductController(Db db, ImageService imageService, IStorage storage, CategoryService categoryService)
    {
        this.db = db;
        this.imageService = imageService;
        this.storage = storage;
        this.categoryService = categoryService;
    }

    public record ListOutput(string Id, string Name, double Price, bool IsDraft);

    [HttpGet("shop/{shopId}"), Authorize]
    public async Task<IActionResult> List(
        [FromRoute] string shopId, [FromQuery] int start = 0, [FromQuery] int count = 10
    )
    {
        var uid = User.Uid();
        var products = await db.Products
                .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
                .Skip(start).Take(count)
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

        if (await PlanLimits.ReachedProductLimit(db, uid)) return Forbid();

        var product = new Product(input.Name, input.SeoSlug, input.ShopId);
        await db.Products.AddAsync(product);

        var saved = await db.Save();
        return saved ? Ok(new ListOutput(product.Id, product.Name, product.Price, product.IsDraft)) : Problem();
    }

    public record ImageOutput(
        string Id, string Key, string Bucket, string Provider
    );

    public record ProductOutput(
        string Id, string Name, double Price, double DiscountPrice, bool IsDiscount, int Amount, bool IsDraft,
        string SeoTitle, string SeoDescription, string SeoSlug,
        string Description, IEnumerable<ImageOutput> Images
    );
    public record struct OnePropertyOutput(string Id, string Key, string Value);
    public record OneOutput(
        ProductOutput Product, int MaxLevel, List<LeveledCategory> Categories, string? CategoryId,
        IEnumerable<OnePropertyOutput> Properties
    );

    [HttpGet("{id}"), Authorize]
    public async Task<IActionResult> One(string id)
    {
        var uid = User.Uid();
        var product = await db.Products
            .Where(x => x.Id == id && x.Shop.OwnerId == uid)
            .Select(x => new
            {
                Product = new ProductOutput(
                    x.Id, x.Name, x.Price, x.DiscountPrice, x.IsDiscount, x.Amount, x.IsDraft,
                    x.SeoTitle, x.SeoDescription, x.SeoSlug, x.Description,
                    x.Images.Select(i => new ImageOutput(i.Id, i.Key, i.Bucket, i.Provider))
                ),
                x.ShopId,
                Properties = x.Properties.Select(p => new OnePropertyOutput(p.Id, p.Key, p.Value))
            })
            .QueryOne();
        if (product == null) return NotFound();

        var productCategory = await db.ProductCategories
            .Where(x => x.ProductId == id).OrderByDescending(x => x.Order)
            .Select(x => x.CategoryId).QueryOne();

        var categories = await db.Categories
            .Where(x => x.ShopId == product.ShopId)
            .QueryMany();

        var sorted = StructuringCategories.SortLeveled(categories);
        var output = new OneOutput(product.Product, sorted.MaxLevel, sorted.List, productCategory, product.Properties);
        return Ok(output);
    }

    public record class UpdateInput(
        string Id, string? Name, double? Price, int? Amount, string? Description,
        string? PreviewImage, string? SeoTitle, string? SeoDescription, string? SeoSlug, double? DiscountPrice, bool IsDiscount
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
        if (input.DiscountPrice != null) product.DiscountPrice = input.DiscountPrice.Value;
        product.IsDiscount = input.IsDiscount;

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpPost("{id}/publish"), Authorize]
    public async Task<IActionResult> Publish([FromRoute] string id)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(x => x.Id == id && x.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        product.IsDraft = false;
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpPost("{id}/unpublish"), Authorize]
    public async Task<IActionResult> Unpublish([FromRoute] string id)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(x => x.Id == id && x.Shop.OwnerId == uid);
        if (product == null) return NotFound();

        product.IsDraft = true;
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var uid = User.Uid();
        var product = await db.Products.QueryOne(p => p.Id == id && p.Shop.OwnerId == uid);

        if (product == null) return Problem();

        bool canDeleteProduct = !await db.OrderProducts.Have(o => o.ProductId == id);

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

        await storage.Delete(image.GetStorageFile());
        return Problem();
    }

    [HttpDelete("image/{id}"), Authorize]
    public async Task<IActionResult> DeleteProductImage([FromRoute] string id)
    {
        var uid = User.Uid();
        var image = await db.ProductImages.QueryOne(pi => pi.Id == id && pi.Product.Shop.OwnerId == uid);
        if (image == null) return NotFound();

        bool isDeleted = await storage.Delete(image.GetStorageFile());
        if (!isDeleted) return Problem();

        db.ProductImages.Remove(image);
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    // CATEGORIES

    public record ChangeProductCategoryInput(string ProductId, string? CategoryId);

    [HttpPatch("categories"), Authorize]
    public async Task<IActionResult> Category([FromBody] ChangeProductCategoryInput input)
    {
        var uid = User.Uid();

        var own = await db.Products.Have(x => x.Id == input.ProductId && x.Shop.OwnerId == uid);
        if (!own) return NotFound();

        // [Parent of new category, another category]
        var currentProductCategories = await db.ProductCategories
            .QueryMany(x => x.ProductId == input.ProductId);

        db.ProductCategories.RemoveRange(currentProductCategories);
        var deleted = await db.Save();
        if (!deleted) return Problem();
        if (input.CategoryId == null) return Ok();

        var category = await db.Categories.QueryOne(x => x.Id == input.CategoryId && x.Shop.OwnerId == uid);
        if (category == null) return Problem();

        var categories = await categoryService.CategoryParentIds(category.ParentId);
        if (categories == null) return Problem();
        // add new category to list
        categories.AddLast(category.Id);

        var newProductCategories = new List<ProductCategory>(categories.Count);
        int order = 0;
        foreach (var categoryId in categories)
        {
            newProductCategories.Add(new ProductCategory(input.ProductId, categoryId, order));
            order += 1;
        }
        await db.AddRangeAsync(newProductCategories);

        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }
}