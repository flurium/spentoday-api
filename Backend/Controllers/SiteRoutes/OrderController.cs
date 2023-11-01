using Backend.Auth;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.Email;
using Lib.EntityFrameworkCore;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/order")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly Db db;

    public OrderController(Db db)
    {
        this.db = db;
    }

    public record OrderOutput(string Id, double Total, int Amount, string Status, DateTime Date);

    [HttpGet("{shopId}/orders")]
    public async Task<IActionResult> Orders([FromRoute] string shopId, [FromQuery] int start = 0)
    {
        var uid = User.Uid();
        var ownShop = await db.Shops.Have(x => x.Id == shopId && uid == x.OwnerId);
        if (!ownShop) return NotFound();

        var orders = await db.Orders
            .Where(x => x.SellerShopId == shopId)
            .Skip(start).Take(10)
            .Select(x => new OrderOutput(
                x.Id,
                x.OrderProducts.Sum(x => x.Amount * x.Price),
                x.OrderProducts.Sum(x => x.Amount),
                x.Status, x.Date
            ))
            .QueryMany();
        return Ok(orders);
    }

    public record StatusInput(string Status);

    [HttpPost("{orderId}/status")]
    public async Task<IActionResult> SetStatus([FromRoute] string orderId, [FromBody] StatusInput input)
    {
        var order = await db.Orders
            .Include(x => x.OrderProducts)
            .QueryOne(x => x.Id == orderId);
        if (order == null) return NotFound();

        if (input.Status == "Скасовано")
        {
            var predicate = PredicateBuilder.New<Product>();
            foreach (var orderProduct in order.OrderProducts)
            {
                predicate = predicate.Or(p => p.Id == orderProduct.ProductId);
            }
            var products = await db.Products
           .Where(predicate)
           .QueryMany();

            foreach (var product in products)
            {
                var orderProduct = order.OrderProducts.FirstOrDefault(x => x.ProductId == product.Id);

                if (orderProduct == null) return NotFound();

                product.Amount += orderProduct.Amount;
            }
        }
        else if ((input.Status == "Готується" || input.Status == "Виконано") && order.Status == "Скасовано")
        {
            var predicate = PredicateBuilder.New<Product>();
            foreach (var orderProduct in order.OrderProducts)
            {
                predicate = predicate.Or(p => p.Id == orderProduct.ProductId);
            }
            var products = await db.Products
           .Where(predicate)
           .QueryMany();

            foreach (var product in products)
            {
                var orderProduct = order.OrderProducts.FirstOrDefault(x => x.ProductId == product.Id);

                if (orderProduct == null) return NotFound();

                product.Amount -= orderProduct.Amount;
            }
        }

        order.Status = input.Status;

        var saved = await db.Save();
        if (!saved) return Problem();
        return Ok();
    }

    public record OrderProductDetails(double Price, int Amount, string Name);
    public record OrderDetails(string Id, string Status, string Email, string Adress, string FullName, string Comment, string Phone, string PostIndex, DateTime Date, List<OrderProductDetails> Details);

    [HttpGet("{orderId}")]
    public async Task<IActionResult> Order([FromRoute] string orderId)
    {
        var details = await db.OrderProducts
           .Where(x => x.OrderId == orderId)
           .Select(x => new OrderProductDetails(x.Price, x.Amount, x.Name))
           .QueryMany();
        if (details == null) return NotFound();
        var order = await db.Orders
            .Where(x => x.Id == orderId)
            .Select(x => new OrderDetails(x.Id, x.Status, x.Email, x.Adress, x.FullName, x.Comment, x.Phone, x.PostIndex, x.Date, details))
            .QueryOne();

        return Ok(order);
    }
}