using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public record LocalListOutput(string Id, string Name, double Price, StorageFile? Image);

    [HttpPost("local")]
    public async Task<IActionResult> LocalList([FromBody] LocalListInput input)
    {
        var products = await db.Products
            // .OwnedBy(input.Domain)
            .Where(x =>
            // !x.IsDraft &&
            input.Ids.Contains(x.Id))
            .Include(x => x.Images)
            .Select(x => new LocalListOutput(
                x.Id, x.Name, x.Price,
                x.Images.Select(x => x.GetStorageFile()).FirstOrDefault()
            ))
            .QueryMany();
        return Ok(products);
    }
}