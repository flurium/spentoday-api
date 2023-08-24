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

        public record class CatalogInput(string Search = "", int Start = 0,  int Count = 10);

        [HttpPost("{domain}"), Authorize]
        public async Task<IActionResult> List([FromRoute]string domain, [FromBody] CatalogInput input)
        {
            var shop = await db.Shops.OwnedBy(domain).QueryOne();

            if (shop == null) return Problem();
            IQueryable<Product> query = db.Products.Where(x => x.ShopId == shop.Id);

            query = query.Where(x => x.Name.Contains(input.Search)).OrderBy(x => x.Name.StartsWith(input.Search)).Skip(input.Start).Take(input.Count);

            var products = await query.QueryMany();

            return Ok(products);
        }
    }
}