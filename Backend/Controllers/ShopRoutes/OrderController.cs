using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.Email;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public record OrderInput(string Email, List<OrderList> Orders, string FullName, string Phone, string Adress, string PostIndex, string Comment);

        [HttpPost("{domain}/new")]
        public async Task<IActionResult> New([FromBody] OrderInput input, [FromRoute] string domain)
        {
            var shop = await db.Shops
            .WithDomain(domain)
            .Include(x => x.Owner)
            .QueryOne();

            if (shop == null) return NotFound();

            var Message = "";
            var newOrder = new Order(input.Email, input.Adress, input.FullName, input.PostIndex, input.Comment);
            foreach (var order in input.Orders)
            {
                var part = $"Назва: {order.Name} Ціна:{order.Price} Кількість: {order.Amount} <br/>";
                Message += part;

                var OrderProduct = new OrderProduct(order.Price, order.Amount, order.Name, order.Id, newOrder.Id);
                await db.OrderProducts.AddAsync(OrderProduct);
            }
            await db.Orders.AddAsync(newOrder);
            var save = await db.Save();
            if (!save) return Problem();

            await email.Send(
             fromEmail: "support@flurium.com",
             fromName: "spentoday",
             toEmails: new List<string>() { input.Email },
             subject: "Замовлення",
             text: $"",
             html: $"Ваше замовлення: {Message}, email продавця ->{shop.Owner.Email}"
            );

            await email.Send(
             fromEmail: "support@flurium.com",
             fromName: "spentoday",
             toEmails: new List<string>() { shop.Owner.Email },
             subject: "Замовлення",
             text: $"",
             html: $"Нове замовлення на {Message} Контакт покупця -  Email: {input.Email} <br/> Телефон: {input.Phone} <br/> Ім'я: {input.FullName} <br/> Поштовий індекс: {input.PostIndex} <br/> Адреса: {input.Adress} <br/> Коммент: {input.Comment} "
            );

            return Ok();
        }
    }
}