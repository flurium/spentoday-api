using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
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

        public record InfoSlug(string Slug, string Title);
        public record InfoData(string Slug, string Title, string Content, string time);

        [HttpGet("{shopDomain}")]
        public async Task<IActionResult> AllPages([FromRoute] string shopDomain)
        {
            var shop = await db.Shops.WithDomain(shopDomain).QueryOne();
            if (shop == null) return NotFound();

            var infoPages = await db.InfoPages
                .Where(x => x.ShopId == shop.Id)
                .Select(x => new InfoSlug(x.Slug, x.Title == "" ? x.Slug : x.Title))
                .QueryMany();

            return Ok(infoPages);
        }

        [HttpGet("info/{slug}")]
        public async Task<IActionResult> GetInfoPage([FromRoute] string slug)
        {
            var infoPages = await db.InfoPages
              .Where(x => x.Slug == slug)
              .Select(x => new InfoData(x.Slug, x.Title == "" ? x.Slug : x.Title, x.Content, x.UpdatedAt.ToString("yyyy-MM-dd HH:mm")))
              .QueryOne();

            return Ok(infoPages);
        }
    }
}