using Backend.Features.Categories;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop/catalog")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly Db db;

    public CatalogController(Db db)
    {
        this.db = db;
    }

    [HttpGet("{domain}/categories")]
    public async Task<IActionResult> Categories([FromRoute] string domain)
    {
        var shopId = await db.Shops.WithDomain(domain)
            .Select(x => x.Id).QueryOne();
        if (shopId == null) return NotFound();

        var categories = await db.Categories
            .Where(x => x.ShopId == shopId)
            .QueryMany();

        return Ok(StructuringCategories.SortLeveled(categories));
    }

    /// <param name="Order">
    /// 0 = No sort | 1 = From cheap | 2 = From expensive
    /// </param>
    public record class CatalogInput(
        string Search = "", int Start = 0, int Count = 10,
        double? Min = null, double? Max = null, int? Order = 0,
        List<string>? Categories = null
    );
    public record ProductsOutput(string Id, string Name, double Price, double DiscountPrice, bool IsDiscount, StorageFile? Image, string Slug);

    [HttpPost("{domain}")]
    public async Task<IActionResult> List([FromRoute] string domain, [FromBody] CatalogInput input)
    {
        var shop = await db.Shops.WithDomain(domain).QueryOne();
        if (shop == null) return Problem();

        var search = input.Search.ToLower();

        IQueryable<Product> query = db.Products
            .Where(x => x.ShopId == shop.Id && x.Name.ToLower().Contains(search) && x.IsDraft == false)
            .Include(x => x.Images)
            .OrderBy(x => x.Name.ToLower().StartsWith(search));

        if (input.Min != null && input.Min > 0) query = query.Where(x => x.Price >= input.Min);

        if (input.Max != null && input.Max > 0) query = query.Where(x => x.Price <= input.Max);

        if (input.Order != 0)
        {
            query = input.Order == 1 ? query.OrderBy(x => x.Price) : query.OrderByDescending(x => x.Price);
        }

        if (input.Categories != null && input.Categories.Count > 0)
        {
            query = query.Where(x => x.ProductCategories.Any(
                x => input.Categories.Contains(x.CategoryId)
            ));
        }

        query = query.Where(x => x.Amount > 0);

        query = query.Skip(input.Start).Take(input.Count);

        var products = await query
            .Select(x => new ProductsOutput(
                x.Id, x.Name, x.Price, x.DiscountPrice, x.IsDiscount,
                x.Images.Select(x => x.GetStorageFile()).FirstOrDefault(), x.SeoSlug
            ))
            .QueryMany();

        return Ok(products);
    }
}