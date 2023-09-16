using Backend.Auth;
using Data;
using Lib.Email;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/order")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly Db db;
    private readonly IEmailSender email;

    public OrderController(Db db, IEmailSender email)
    {
        this.db = db;
        this.email = email;
    }

    public record OrderOutput(string Id, double Total, int Amount, string Status, DateTime Date);

    [HttpGet("{shopId}/orders")]
    public async Task<IActionResult> Orders([FromRoute] string shopId)
    {
        var uid = User.Uid();
        var shop = await db.Shops.QueryOne(x => x.Id == shopId && uid == x.OwnerId);
        if (shop == null) return NotFound();

        var orders = await db.Orders
            .Where(x => x.SellerShopId == shopId)
            .Select(x => new OrderOutput(x.Id, x.OrderProducts.Sum(x => x.Amount * x.Price), x.OrderProducts.Sum(x => x.Amount), x.Status, x.Date))
            .QueryMany();
        return Ok(orders);
    }

    public record StatusInput(string Status);

    [HttpPost("{orderId}/status")]
    public async Task<IActionResult> SetStatus([FromRoute] string orderId, [FromBody] StatusInput input)
    {
        var order = await db.Orders
            .QueryOne(x => x.Id == orderId);
        if (order == null) return NotFound();
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