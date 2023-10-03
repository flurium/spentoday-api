using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes
{
    [Route("v1/shop/about")]
    [ApiController]
    public class InfoPageController : ControllerBase
    {
        private readonly Db db;

        public InfoPageController(Db db)
        {
            this.db = db;
        }

        public record InfoSlug(string Slug);
        public record InfoData(string Slug, string Content);

        [HttpGet("{shopDomain}")]
        public async Task<IActionResult> AllPages([FromRoute] string shopDomain)
        {
            var shop = await db.Shops.WithDomain(shopDomain).QueryOne();
            if (shop == null) return NotFound();

            var infoPages = await db.InfoPages
                .Where(x => x.ShopId == shop.Id)
                .Select(x => new InfoSlug(x.Slug))
                .QueryMany();

            return Ok(infoPages);
        }

        [HttpGet("info/{slug}")]
        public async Task<IActionResult> GetInfoPage([FromRoute] string slug)
        {
            var infoPages = await db.InfoPages
              .Where(x => x.Slug == slug)
              .Select(x => new InfoData(x.Slug, x.Content))
              .QueryOne();

            return Ok(infoPages);
        }
    }
}