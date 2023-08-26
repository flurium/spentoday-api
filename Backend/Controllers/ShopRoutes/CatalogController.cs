using Backend.Services;
using Data;
using Data.Models.ProductTables;
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

        //[HttpPost("{domain}"), Authorize]
        //public async Task<IActionResult> List([FromRoute] string domain, [FromBody] string search, [FromBody] int start = 0, [FromBody] int count = 10)
        //{
        //    var shop = await db.Shops.QueryOne(x => x.Domains.Any(x => x.IsOwned(domain)));
        //    if (shop == null) return Problem();
        //    IQueryable<Product> query = db.Products.Where(x => x.ShopId == shop.Id);

        //    query = query.Where(x => x.Name.Contains(search)).OrderBy(x => x.Name.StartsWith(search));

        //    var products = await query.ToListAsync();

        //    return Ok(products);
        //}
    }
}