using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly Db db;

    public CartController(Db db)
    {
        this.db = db;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var products = await db.Products.QueryMany();
        return Ok(products);
    }

    public record LocalListInput(List<string> Ids, string Domain);
    public record LocalListOutput(string Id, string Name, double Price);

    [HttpPost("local")]
    public async Task<IActionResult> LocalList([FromBody] LocalListInput input)
    {
        var products = await db.Products
            .OwnedBy(input.Domain)
            .Where(x => !x.IsDraft && input.Ids.Contains(x.Id))
            .Select(x => new LocalListOutput(x.Id, x.Name, x.Price))
            .QueryMany();
        return Ok(products);
    }
}