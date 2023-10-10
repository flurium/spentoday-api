using Data;
using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Lib.Email;
using Lib.EntityFrameworkCore;
using LinqKit;
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

        public record ProductInputList(string Id, int Amount);
        public record OrderInput(string Email, List<ProductInputList> Products, string FullName, string Phone, string Adress, string PostIndex, string Comment);

        public class ProductList
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public int Amount { get; set; }
        }

        [HttpPost("{domain}/new")]
        public async Task<IActionResult> New([FromBody] OrderInput input, [FromRoute] string domain)
        {
            var shop = await db.Shops
            .WithDomain(domain)
            .Include(x => x.Owner)
            .QueryOne();

            if (shop == null) return NotFound();

            var predicate = PredicateBuilder.New<Product>();
            foreach (var product in input.Products)
            {
                predicate = predicate.Or(p => p.Id == product.Id);
            }

            var products = await db.Products
           .Where(x => x.ShopId == shop.Id)
           .Where(predicate)
           .Select(p => new ProductList()
           {
               Id = p.Id,
               Name = p.Name,
               Price = p.IsDiscount ? p.DiscountPrice : p.Price,
               Amount = 0
           }
           )
           .QueryMany();

            foreach (var product in products)
            {
                var inputProduct = input.Products.FirstOrDefault(x => x.Id == product.Id);
                if (inputProduct == null) return NotFound();
                product.Amount = inputProduct.Amount;
            }

            var Message = "";
            var newOrder = new Order(input.Email, input.Adress, input.FullName, input.PostIndex, input.Comment, "Готується", shop.Id, input.Phone);
            foreach (var product in products)
            {
                var part = $"Назва: {product.Name} Ціна:{product.Price} Кількість: {product.Amount} <br/>";
                Message += part;
                var OrderProduct = new OrderProduct(product.Price, product.Amount, product.Name, product.Id, newOrder.Id);
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