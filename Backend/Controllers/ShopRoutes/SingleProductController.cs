using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop/single")]
[ApiController]
public class SingleProductController : ControllerBase
{
    private readonly Db db;
    private readonly IStorage storj;

    public SingleProductController(Db db, IStorage storj)
    {
        this.db = db;
        this.storj = storj;
    }

    public record ProductOutput(
        string Id, string Name, double Price, int Amount,
        string SeoTitle, string SeoDescription, string SeoSlug,
        string Description, List<string> Images
    );

    public record ProductItemOutput(
        string Id, string Name, double Price,
        string? Image, string SeoSlug
    );

    public record Output(ProductOutput Product, List<ProductItemOutput> Products);

    [HttpGet("{domain}/{slugOrId}/product")]
    public async Task<IActionResult> SingleProduct(
        [FromRoute] string domain, [FromRoute] string slugOrId
    )
    {
        var product = await db.Products
            .WithDomain(domain)
            .Where(x => x.SeoSlug == slugOrId || x.Id == slugOrId)
            .Select(x => new ProductOutput(
                x.Id, x.Name, x.Price, x.Amount,
                x.SeoTitle, x.SeoDescription, x.SeoSlug, x.Description,
                x.Images.Select(i => storj.Url(i.GetStorageFile())).ToList()
            ))
            .QueryOne();
        if (product == null) return NotFound();

        var similar = await SimilarProducts(domain, product.Name);

        var output = new Output(product, similar);
        return Ok(output);
    }

    [NonAction]
    public async Task<List<ProductItemOutput>> SimilarProducts(string domain, string name)
    {
        var keywords = name.Split(' ').Select(x => x.ToLower()).ToList();

        var keywordsPredicate = PredicateBuilder.New<Product>();
        foreach (var keyword in keywords)
        {
            keywordsPredicate = keywordsPredicate.Or(p => p.Name.ToLower().Contains(keyword));
        }

        var products = await db.Products
            .Where(x => x.Shop.Domains.Any(x => x.Domain == domain && x.Verified) && x.Name != name)
            .Where(keywordsPredicate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Price,
                Image = x.Images.OrderBy(i => i.Id == x.PreviewImage).FirstOrDefault(),
                x.SeoSlug
            })
            .Take(4)
            .QueryMany();

        return products.Select(x =>
        {
            var image = x.Image == null ? null : storj.Url(x.Image.GetStorageFile());
            return new ProductItemOutput(x.Id, x.Name, x.Price, image, x.SeoSlug);
        }).ToList();
    }
}