using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/domains")]
[ApiController]
public class DomainController : ControllerBase
{
    private readonly Db db;
    private readonly DomainService domainService;
    private readonly BackgroundQueue background;

    public DomainController(Db db, DomainService domainService, BackgroundQueue background)
    {
        this.db = db;
        this.domainService = domainService;
        this.background = background;
    }

    public record DomainOutput(string Domain, bool GotStatus, List<DomainVerification>? Verifications);

    [HttpGet("{shopId}"), Authorize]
    public async Task<IActionResult> ShopDomains([FromRoute] string shopId)
    {
        var uid = User.FindFirst(Jwt.Uid)!.Value;
        var domains = await db.ShopDomains
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
            .Select(x => x.Domain)
            .QueryMany();

        List<DomainOutput> output = new(domains.Count);
        for (int i = 0; i < domains.Count; ++i)
        {
            var domain = domains[i];
            var result = await domainService.GetDomainInfo(domain);

            output.Add(result == null
                ? new DomainOutput(domain, false, null)
                : new DomainOutput(domain, true, result.Verification)
            );
        }

        return Ok(output);
    }

    public record AddDomainInput(string Domain);

    [HttpPost("{shopId}"), Authorize]
    public async Task<IActionResult> AddDomain([FromRoute] string shopId, [FromBody] AddDomainInput input)
    {
        var domain = input.Domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var uid = User.Uid();
        if (domain.EndsWith(".spentoday.com"))
        {
            var hasFreeDomain = await db.ShopDomains
                .Have(x => x.Domain.EndsWith(".spentoday.com") && x.ShopId == shopId && x.Shop.OwnerId == uid);
            if (hasFreeDomain) return Conflict("has-free-domain");
        }

        var domainTaken = await db.ShopDomains.Have(x => x.Domain == domain);
        if (domainTaken) return Conflict("domain-taken");

        var userOwnShop = await db.Shops.Have(x => x.OwnerId == uid && x.Id == shopId);
        if (!userOwnShop) return Forbid();

        var domainResponse = await domainService.AddDomainToShop(domain);
        if (domainResponse == null) return Problem();

        await db.ShopDomains.AddAsync(new ShopDomain(domain, shopId));
        var saved = await db.Save();

        return saved ? Ok(new DomainOutput(domain, true, domainResponse.Verification)) : Problem();
    }

    [HttpDelete("{shopId}/{domain}"), Authorize]
    public async Task<IActionResult> DeleteDomain([FromRoute] string shopId, [FromRoute] string domain)
    {
        domain = domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var uid = User.FindFirst(Jwt.Uid)!.Value;
        var shopDomain = await db.ShopDomains.QueryOne(x => x.Domain == domain && x.ShopId == shopId && x.Shop.OwnerId == uid);
        if (shopDomain == null) return NotFound();

        db.ShopDomains.Remove(shopDomain);
        var saved = await db.Save();
        if (!saved) return Problem();

        var removed = await domainService.RemoveDomainFromShop(domain);
        if (!removed)
        {
            background.Enqueue(async provider =>
            {
                using var scope = provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<DomainService>();
                await domainService.RemoveDomainFromShop(domain);
            });
        }

        return Ok();
    }

    [HttpPatch("{domain}/verify"), Authorize]
    public async Task<IActionResult> VerifyDomain([FromRoute] string domain)
    {
        domain = domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var verified = await domainService.VerifyDomain(domain);
        if (verified) return Ok();

        var info = await domainService.GetDomainInfo(domain);
        if (info == null) return Accepted(new DomainOutput(domain, false, null));
        return Accepted(new DomainOutput(domain, true, info.Verification));
    }
}