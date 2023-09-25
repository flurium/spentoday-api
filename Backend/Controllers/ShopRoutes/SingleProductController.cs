using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Backend.Auth;
using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Lib.Storage.Services;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Backend.Controllers.ShopRoutes.ShopController;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
        public async Task<IActionResult> SingleProduct([FromRoute] string domain, [FromRoute] string slugOrId)
        {
            var shopDomain = await db.ShopDomains.QueryOne(x => x.Domain == domain);
            if (shopDomain == null) return null;

            var product = await db.Products
                .Where(x => (x.Id == slugOrId || x.SeoSlug == slugOrId) && x.ShopId == shopDomain.ShopId)
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

            var mayWays = await TakeProducts(domain, product.Product.Name);

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
        public async Task<List<ProductItemOutput>?> TakeProducts(string domain, string name, int start = 0)
        {
            var shopDomain = await db.ShopDomains.QueryOne(x=> x.Domain == domain);
            if (shopDomain == null) return null;

            string[] keywords = name.Split(' ');

            var predicate = PredicateBuilder.New<Product>(p => p.ShopId == shopDomain.ShopId && p.Name != name);
            var predicate_words = PredicateBuilder.New<Product>();

            foreach (var keyword in keywords)
            {
                predicate_words = predicate_words.Or(p=>p.Name.ToLower().Contains(keyword.ToLower()));
            }

            predicate = predicate.And(predicate_words);

            var products = await db.Products
                .Where(predicate)
                .Include(x=>x.Images)
                .Distinct()
                .Take(4)
                .QueryMany();

            var mayWays = products
            .Select(x =>
            {
                if (x.PreviewImage != null) {
                  var previewImage = x.Images.OrderByDescending(p => p.Id == x.PreviewImage).FirstOrDefault();
                  if (previewImage != null) return new ProductItemOutput(x.Id, x.Name, x.Price, storj.Url(previewImage.GetStorageFile()), x.SeoSlug);
                }

                var image = x.Images.FirstOrDefault();
                if (image != null) return new ProductItemOutput(x.Id, x.Name, x.Price, storj.Url(image.GetStorageFile()), x.SeoSlug);
                
                return new ProductItemOutput(x.Id, x.Name, x.Price, null , x.SeoSlug);
            })
            .ToList();

            return mayWays;
        }
    }
}
