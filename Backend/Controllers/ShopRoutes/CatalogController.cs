using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.ShopRoutes
{
    [Route("v1/shop/catalog")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly Db db;
        private readonly ImageService imageService;
        private readonly IStorage storage;
        private readonly CategoryService categoryService;

        public CatalogController(Db db, ImageService imageService, IStorage storage, CategoryService categoryService)
        {
            this.db = db;
            this.imageService = imageService;
            this.storage = storage;
            this.categoryService = categoryService;
        }

        public record class CatalogInput(string Search = "", int Start = 0,  int Count = 10, int? Min = null, int? Max = null, int? Order = 0);
        public record ProductsOutput(string Id, string Name, double Price, StorageFile? Image);

        [HttpPost("{domain}")]
        public async Task<IActionResult> List([FromRoute]string domain, [FromBody] CatalogInput input)
        {
            var shop = await db.Shops.OwnedBy(domain).QueryOne();

            if (shop == null) return Problem();
            //IQueryable<Product> query = db.Products.Where(x => x.ShopId == shop.Id).Include(x => x.Images);

            IQueryable<Product> query = db.Products.Where(x => x.ShopId == shop.Id && x.Name.Contains(input.Search))
                .Include(x => x.Images)
                .OrderBy(x => x.Name.StartsWith(input.Search));              

            if (input.Min != null && input.Min > 0) query = query.Where(x => x.Price >= input.Min);

            if (input.Max != null && input.Max > 0) query = query.Where(x => x.Price <= input.Max);

            if (input.Order != 0)
            {
                query = input.Order == 1 ? query.OrderBy(x => x.Price) : query.OrderBy(x => x.Price).Reverse();
            }

            query = query.Skip(input.Start).Take(input.Count);

            var products = await query.Select(x => new ProductsOutput(x.Id, x.Name, x.Price, x.Images.Select(x => x.GetStorageFile()).FirstOrDefault())).QueryMany();

            return Ok(products);
        }
    }
}