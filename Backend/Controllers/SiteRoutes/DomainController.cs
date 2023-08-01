using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    public record ShopDomainOutput(string Domain);

    [HttpGet("{shopId}")]
    [Authorize]
    public async Task<IActionResult> ShopDomains([FromRoute] string shopId)
    {
        var uid = User.FindFirst(Jwt.Uid)!.Value;
        var domains = await db.ShopDomains
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
            .Select(x => new ShopDomainOutput(x.Domain))
            .QueryMany();
        return Ok(domains);
    }

    public record AddDomainInput(string Domain);
    public record AddDomainOutput(string Domain, List<DomainVerification>? Verifications);

    [HttpPost("{shopId}")]
    [Authorize]
    public async Task<IActionResult> AddDomain([FromRoute] string shopId, [FromBody] AddDomainInput input)
    {
        var domain = input.Domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var domainTaken = await db.ShopDomains.AnyAsync(x => x.Domain == domain).ConfigureAwait(false);
        if (domainTaken) return Conflict();

        var uid = User.FindFirst(Jwt.Uid)!.Value;
        var userOwnShop = await db.Shops.AnyAsync(x => x.OwnerId == uid && x.Id == shopId).ConfigureAwait(false);
        if (!userOwnShop) return Forbid();

        var domainResponse = await domainService.AddDomainToShop(domain);
        if (domainResponse == null) return Problem();

        await db.ShopDomains.AddAsync(new ShopDomain(domain, shopId));
        var saved = await db.Save();

        return saved ? Ok(new AddDomainOutput(domain, domainResponse.Verification)) : Problem();
    }

    [HttpDelete("{shopId}/{domain}")]
    [Authorize]
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

    [HttpPatch("{domain}/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyDomain([FromRoute] string domain)
    {
        domain = domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var res = await domainService.VerifyDomain(domain);
        if (res == null) return Problem();

        if (res.Verified) return Ok();
        return Accepted(new AddDomainOutput(domain, res.Verification));
    }
}