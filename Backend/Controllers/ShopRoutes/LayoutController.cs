using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Linq;

using System.ComponentModel.DataAnnotations;


namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop")]
[ApiController]
public class LayoutController : ControllerBase
{
    private readonly Db db;

    public LayoutController(Db db)
    {
        this.db = db;
    }

    public record LayoutCategory(string Id, string Name);
    public record LayoutPage(string Slug, string Name);
    public record LayoutSocialMedia(string Name, string Link);
    public record LayoutShop(string Id, string Name,
        IEnumerable<LayoutCategory> Categories, IEnumerable<LayoutPage> Pages,
        IEnumerable<LayoutSocialMedia> SocialMediaLinks
    );

    [HttpGet("{domain}/layout")]
    public async Task<IActionResult> Layout([FromRoute] string domain)
    {
        var shop = await db.Shops
            .WithDomain(domain)
            .Include(x => x.Categories)
            .Include(x => x.InfoPages)
            .Select(x => new LayoutShop(
                x.Id, x.Name,
                x.Categories.Select(x => new LayoutCategory(x.Id, x.Name)),
                x.InfoPages.Select(x => new LayoutPage(x.Slug, x.Title)),
                x.SocialMediaLinks.Select(x => new LayoutSocialMedia(x.Name, x.Link))
            ))
            .QueryOne();
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    public class SubscribeInput
    {
        public string Email { get; set; }
        public string ShopId { get; set; }

        public SubscribeInput(string email, string shopId)
        {
            ShopId = shopId;
            Email = email;
        }
    };

    [NonAction]
    public bool IsValidEmail(string email) => new EmailAddressAttribute().IsValid(email);

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeInput input)
    {
        input.Email = input.Email.Trim();
        if (!IsValidEmail(input.Email)) return BadRequest();

        input.ShopId = input.ShopId.Trim();
        if (string.IsNullOrEmpty(input.ShopId)) return NotFound();

        var shopExist = await db.Shops.Have(x => x.Id == input.ShopId);
        if (!shopExist) return NotFound();

        var subscriptionExist = await db.ShopSubscriptions.Have(x => x.Email == input.Email && x.ShopId == input.ShopId);
        if (subscriptionExist) return Ok();

        var subscription = new Subscription(input.Email, input.ShopId);
        await db.ShopSubscriptions.AddAsync(subscription);
        var saved = await db.Save();

        return saved ? Ok() : Problem();
    }
}