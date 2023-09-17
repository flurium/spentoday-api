using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Backend.Controllers.SiteRoutes.DashboardController;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/subscriptions")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly Db db;

    public SubscriptionController(Db db)
    {
        this.db = db;
    }

    public record struct ListSubscription(string Id, string Email, DateTime Date);

    [HttpGet("{shopId}"), Authorize]
    public async Task<IActionResult> List(
        [FromRoute] string shopId,
        [FromQuery] string? search = null,
        [FromQuery] int start = 0
    )
    {
        var uid = User.Uid();

        // Order by break pagination
        //.OrderByDescending(x => x.Email.StartsWith(sqlSearch))

        string sqlSearch = search ?? "";
        var subscriptions = await db.ShopSubscriptions
            .OrderByDescending(x => x.Date)
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid && x.Email.Contains(sqlSearch))
            .Skip(start)
            .Take(10)
            .Select(x => new ListSubscription(x.Id, x.Email, x.Date))
            .QueryMany();

        return Ok(subscriptions);
    }

    [HttpDelete("{subscriptionId}"), Authorize]
    public async Task<IActionResult> Unsubscribe([FromRoute] string subscriptionId)
    {
        var uid = User.Uid();

        var subscription = await db.ShopSubscriptions
            .QueryOne(x => x.Id == subscriptionId && x.Shop.OwnerId == uid);
        if (subscription == null) return NotFound();

        db.ShopSubscriptions.Remove(subscription);
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    public class SubscriptionInput
    {
        public string Email { get; set; }
        public string ShopId { get; set; }

        public SubscriptionInput(string email, string shopId)
        {
            ShopId = shopId;
            Email = email;
        }

        public bool IsValid()
        {
            Email = Email.Trim();
            if (string.IsNullOrEmpty(Email)) return false;
            if (!Email.IsValidEmail()) return false;
            ShopId = ShopId.Trim();
            if (string.IsNullOrEmpty(ShopId)) return false;
            return true;
        }
    };

    [HttpPost, Authorize]
    public async Task<IActionResult> Add([FromBody] SubscriptionInput input)
    {
        var uid = User.Uid();
        if (!input.IsValid()) return BadRequest();

        var own = await db.Shops.Have(x => x.Id == input.ShopId && x.OwnerId == uid);
        if (!own) return Forbid();

        var exists = await db.ShopSubscriptions
            .Have(x => x.ShopId == input.ShopId && x.Email == input.Email);
        if (exists) return Conflict();

        var subscription = new Subscription(input.Email, input.ShopId);
        await db.ShopSubscriptions.AddAsync(subscription);
        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok(new ListSubscription(subscription.Id, subscription.Email, subscription.Date));
    }
}