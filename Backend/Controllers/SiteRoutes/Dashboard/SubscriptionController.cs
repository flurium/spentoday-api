using Backend.Auth;
using Data;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes.Dashboard;

[Route("v1/site/subscriptions")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private Db db;

    public SubscriptionController(Db db)
    {
        this.db = db;
    }

    public record struct ListSubscription(string Id, string Email);

    [HttpGet("{shopId}"), Authorize]
    public async Task<IActionResult> List([FromRoute] string shopId)
    {
        var uid = User.Uid();

        var subscriptions = await db.ShopSubscriptions
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
            .Select(x => new ListSubscription(x.Id, x.Email))
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
}