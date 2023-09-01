using Backend.Auth;
using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.Email;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Backend.Controllers.ShopRoutes.CartController;

namespace Backend.Controllers.ShopRoutes
{
    [Route("v1/shop/order")]
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

        public record OrderList(string Name, double Price, string Id, int Amount);
        public record OrderInput(string Email, List<OrderList> Orders,string FullName, string Phone, string Adress, string PostIndex, string Comment);

        [HttpPost("{domain}/new")]
        public async Task<IActionResult> New([FromBody] OrderInput input, [FromRoute] string domain)
        {    
            var shop = await db.Shops
            .WithDomain(domain)
            .Include(x=>x.Owner)
            .QueryOne();

            if (shop == null) return NotFound();

            var Message = "";
            var newOrder = new Order(input.Email, input.Adress, input.FullName, input.PostIndex, input.Comment);
            foreach(var order in input.Orders)
            {
                var part = $"Name: {order.Name} Price:{order.Price} Amount: {order.Amount} \n";
                Message += part;

                var OrderProduct = new OrderProduct(order.Price,order.Amount,order.Name, order.Id, newOrder.Id);
                await db.OrderProducts.AddAsync(OrderProduct);
            }
            await db.Orders.AddAsync(newOrder);
            var save = await db.Save();
            if(!save) return Problem();

            await email.Send(
             fromEmail: "support@flurium.com",
             fromName: "spentoday",
             toEmails: new List<string>() { input.Email },
             subject: "Order",
             text: $"Your order is {Message}, seller email ->{shop.Owner.Email}",
             html: $""
            );

            await email.Send(
             fromEmail: "support@flurium.com",
             fromName: "spentoday",
             toEmails: new List<string>() { shop.Owner.Email },
             subject: "Order",
             text: $"new Order to {Message}\n customer`s contacts :\n Email: {input.Email}\n Phone: {input.Phone}\n {input.FullName} \n PostIndex:{input.PostIndex} \n Adress:{input.Adress} \n Comment{input.Comment} ",
             html: $""
            );

            return Ok();
        }
    }
}
