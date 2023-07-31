using Data;
using Data.Models.ProductTables;
using Lib;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Dashboard
{
    [Route("v1/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Db db;

        public CategoryController(Db db)
        {
            this.db = db;
        }

        public record ShopCategoryOutput(string Id, string Name);

        [HttpGet("{shopId}")]
        [Authorize]
        public async Task<IActionResult> ShopCategories([FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid)?.Value;
            if (uid == null) Forbid();

            var categories = await db.Categories
                .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
                .Select(x => new ShopCategoryOutput(x.Id, x.Name))
                .QueryMany();

            return Ok(categories);
        }

        public record AddCategoryInput(string Name, string ShopId);
        public record AddCategoryOutput(string Id, string Name);

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryInput input)
        {
            var uid = User.FindFirst(Jwt.Uid)?.Value;
            if (uid == null) Forbid();

            var ownShop = await db.Shops.Have(x => x.Id == input.ShopId && x.OwnerId == uid);
            if (!ownShop) return Forbid();

            var category = new Category(input.Name, input.ShopId);
            await db.Categories.AddAsync(category);

            var saved = await db.Save();
            return saved ? Ok(new AddCategoryOutput(category.Id, category.Name)) : Problem();
        }
    }
}