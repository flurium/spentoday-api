using Backend.Auth;
using Backend.Features.Domains;
using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/domains")]
[ApiController]
public class DomainController : ControllerBase
{
    private readonly Db db;
    private readonly VercelDomainApi vercelDomainApi;
    private readonly BackgroundQueue background;

    public DomainController(Db db, VercelDomainApi domainService, BackgroundQueue background)
    {
        this.db = db;
        this.vercelDomainApi = domainService;
        this.background = background;
    }

    [HttpGet("{shopId}"), Authorize]
    public async Task<IActionResult> ShopDomains([FromRoute] string shopId)
    {
        var uid = User.Uid();
        var dbDomains = await db.ShopDomains
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
            .QueryMany();

        List<DomainStatus> output = new(dbDomains.Count);
        for (int i = 0; i < dbDomains.Count; ++i)
        {
            var dbDomain = dbDomains[i];
            var status = await DomainService.GetStatusAndSync(vercelDomainApi, db, dbDomain);
            output.Add(status);
        }

        return Ok(output);
    }

    public record AddDomainInput(string Domain);

    // TODO: remake
    [HttpPost("{shopId}"), Authorize]
    public async Task<IActionResult> AddDomain([FromRoute] string shopId, [FromBody] AddDomainInput input)
    {
        var domain = input.Domain.Trim();
        if (string.IsNullOrEmpty(domain) || domain.EndsWith(".flurium.com")) return BadRequest();

        var uid = User.Uid();
        if (domain.EndsWith(".spentoday.com"))
        {
            var hasFreeDomain = await db.ShopDomains
                .Have(x => x.Domain.EndsWith(".spentoday.com") && x.ShopId == shopId && x.Shop.OwnerId == uid);
            if (hasFreeDomain) return Conflict("has-free-domain");
        }

        var domainTaken = await db.ShopDomains.Have(x => x.Domain == domain && x.Verified);
        if (domainTaken) return Conflict("domain-taken");

        var userOwnShop = await db.Shops.Have(x => x.OwnerId == uid && x.Id == shopId);
        if (!userOwnShop) return Forbid();

        var projectDomain = await vercelDomainApi.AddDomain(domain);
        if (projectDomain == null) return Problem();

        var dbDomain = new ShopDomain(domain, shopId, false);
        await db.ShopDomains.AddAsync(dbDomain);
        var saved = await db.Save();
        if (!saved) return Problem();

        var status = await DomainService.GetStatusAndSync(vercelDomainApi, db, dbDomain);
        return Ok(status);
    }

    [HttpDelete("{shopId}/{domain}"), Authorize]
    public async Task<IActionResult> DeleteDomain([FromRoute] string shopId, [FromRoute] string domain)
    {
        domain = domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var uid = User.Uid();
        var shopDomain = await db.ShopDomains.QueryOne(
            x => x.Domain == domain && x.ShopId == shopId && x.Shop.OwnerId == uid
        );
        if (shopDomain == null) return NotFound();

        db.ShopDomains.Remove(shopDomain);
        var saved = await db.Save();
        if (!saved) return Problem();

        var removed = await vercelDomainApi.RemoveDomain(domain);
        if (!removed)
        {
            background.Enqueue(async provider =>
            {
                using var scope = provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<VercelDomainApi>();
                await vercelDomainApi.RemoveDomain(domain);
            });
        }

        return Ok();
    }

    public record VerifyInput(string Domain, string ShopId);

    // TODO: remake

    [HttpPatch("verify"), Authorize]
    public async Task<IActionResult> VerifyDomain([FromBody] VerifyInput input)
    {
        var domain = input.Domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var takenDomain = await db.ShopDomains
            .QueryOne(x => x.Domain == domain && x.Verified && x.ShopId != input.ShopId);
        if (takenDomain != null)
        {
            var projectDomain = await vercelDomainApi.GetProjectDomain(takenDomain.Domain);
            if (projectDomain == null) return Problem();

            if (!projectDomain.Verified)
            {
                var synced = await DomainService.SyncDbDomainVerification(db, takenDomain, false);
                if (!synced) return Problem();
            }
            else
            {
                var domainConfiguration = await vercelDomainApi.GetDomainConfiguration(takenDomain.Domain);
                if (domainConfiguration == null) return Problem();

                if (domainConfiguration.Misconfigured)
                {
                    var synced = await DomainService.SyncDbDomainVerification(db, takenDomain, false);
                    if (!synced) return Problem();
                }
            }
            return Conflict();
        }

        var uid = User.Uid();
        var dbDomain = await db.ShopDomains.QueryOne(
            x => x.Domain == domain && x.ShopId == input.ShopId && x.Shop.OwnerId == uid
        );
        if (dbDomain == null) return NotFound();

        var verified = await vercelDomainApi.VerifyDomain(domain);
        if (verified == null) return Problem();

        var state = await DomainService.GetStatusAndSync(vercelDomainApi, db, dbDomain);
        return Ok(state);
    }
}