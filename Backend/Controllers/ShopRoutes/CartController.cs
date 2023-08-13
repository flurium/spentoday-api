using Data;
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
        // TODO: check shop

        var products = await db.Products
            .Where(x => !x.IsDraft
                && input.Ids.Contains(x.Id)
            //    && x.Shop.Domains.Any(x => x.Domain == input.Domain)
            )
            .Select(x => new LocalListOutput(x.Id, x.Name, x.Price))
            .QueryMany();
        return Ok(products);
    }
}