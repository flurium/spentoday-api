using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Backend.Auth;
using Data;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Lib.Storage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Backend.Controllers.ShopRoutes
{
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
                .Where(x => (x.Id == slugOrId || x.SeoSlug == slugOrId))
                .Select(x => new
                {
                    Product = new ProductOutput(
                        x.Id, x.Name, x.Price, x.Amount,
                        x.SeoTitle, x.SeoDescription, x.SeoSlug, x.Description,
                        x.Images.Select(i => storj.Url(i.GetStorageFile())).ToList()
                    )
                })
                .QueryOne();

            if (product == null) return NotFound();

            var mayWays = await TakeProductsRoma(domain, product.Product.Name);

            var output = new Output(product.Product, mayWays);

            return Ok(output);
        }

        /*[HttpGet("{domain}/{name}/products")]
        public async Task<IActionResult> Products([FromRoute] string domain, [FromRoute] string name, [FromQuery] int start)
        {
           var mays = await TakeProducts(domain,name,start);
            return Ok(mays);
        }*/

        [NonAction]
        public async Task<List<ProductItemOutput>?> TakeProductsRoma(string domain, string name, int start = 0)
        {
            try
            {
                var keywords = name.Split(' ').Select(x => x.ToLower()).ToList();
                var products = await db.Products
                    .Where(x => x.Shop.Domains.Any(x => x.Domain == domain && x.Verified))
                    .Where(product => keywords.Any(keyword => EF.Functions.Like(product.Name, $"%{keyword}%")))
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        [NonAction]
        public async Task<List<ProductItemOutput>?> TakeProducts(string domain, string name, int start = 0)
        {
            var shopDomain = await db.ShopDomains.QueryOne(x => x.Domain == domain);
            string defaultUrl = "data:image/png;base64,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";

            if (shopDomain == null) return null;

            string[] keywords = name.Split(' ');

            var mayWays = new List<ProductItemOutput>();
            foreach (var keyword in keywords)
            {
                var products = await db.Products
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()) && x.ShopId == shopDomain.ShopId)
                .Include(x => x.Images)
                .QueryMany();

                var append = products.Select(x =>
                {
                    if (x.PreviewImage != null) return new ProductItemOutput(x.Id, x.Name, x.Price, x.PreviewImage, x.SeoSlug);
                    else if (x.Images != null)
                    {
                        var image = x.Images.FirstOrDefault();
                        if (image != null) return new ProductItemOutput(x.Id, x.Name, x.Price, storj.Url(image.GetStorageFile()), x.SeoSlug);
                    }
                    return new ProductItemOutput(x.Id, x.Name, x.Price, defaultUrl, x.SeoSlug);
                }
                )
                .ToList();

                mayWays.AddRange(append);
            }

            mayWays = mayWays.Distinct().ToList();

            mayWays = mayWays.Where(x => x.Name != name).Take(4).ToList();

            return mayWays;
        }
    }
}