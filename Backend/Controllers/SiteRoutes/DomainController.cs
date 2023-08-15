using Backend.Auth;
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
    private readonly DomainService domainService;
    private readonly BackgroundQueue background;

    public DomainController(Db db, DomainService domainService, BackgroundQueue background)
    {
        this.db = db;
        this.domainService = domainService;
        this.background = background;
    }

    public class DomainOutput
    {
        public string Domain { get; }
        public string Status { get; }
        public List<DomainVerification>? Verifications { get; }

        private DomainOutput(string domain, string status, List<DomainVerification>? verifications = null)
        {
            Domain = domain;
            Status = status;
            Verifications = verifications;
        }

        public static DomainOutput NoStatus(string domain) => new(domain, "no-status");

        public static DomainOutput Taken(string domain) => new(domain, "taken");

        public static DomainOutput Verified(string domain) => new(domain, "verified");

        public static DomainOutput NotVerified(string domain, List<DomainVerification> verifications) => new(domain, "not-verified", verifications);
    }

    [HttpGet("{shopId}"), Authorize]
    public async Task<IActionResult> ShopDomains([FromRoute] string shopId)
    {
        var uid = User.Uid();
        var domains = await db.ShopDomains
            .Where(x => x.ShopId == shopId && x.Shop.OwnerId == uid)
            .QueryMany();

        List<DomainOutput> output = new(domains.Count);
        for (int i = 0; i < domains.Count; ++i)
        {
            var domain = domains[i];
            var result = await domainService.GetDomainInfo(domain.Domain);

            if (result == null)
            {
                output.Add(DomainOutput.NoStatus(domain.Domain));
                continue;
            }

            if (!domain.Verified && result.Verified)
            {
                // domain is taken by another shop
                output.Add(DomainOutput.Taken(domain.Domain));
                continue;
            }

            if (domain.Verified && !result.Verified)
            {
                // verification changed
                domain.Verified = false;
                await db.Save();
            }

            output.Add(result.Verification == null
                ? DomainOutput.Verified(domain.Domain)
                : DomainOutput.NotVerified(domain.Domain, result.Verification)
            );
        }

        return Ok(output);
    }

    public record AddDomainInput(string Domain);

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

        var domainResponse = await domainService.AddDomainToShop(domain);
        if (domainResponse == null) return Problem();

        await db.ShopDomains.AddAsync(new ShopDomain(domain, shopId, domainResponse.Verified));
        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok(domainResponse.Verification == null
            ? DomainOutput.Verified(domain)
            : DomainOutput.NotVerified(domain, domainResponse.Verification)
        );
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

    public record VerifyInput(string Domain, string ShopId);

    [HttpPatch("verify"), Authorize]
    public async Task<IActionResult> VerifyDomain([FromBody] VerifyInput input)
    {
        var domain = input.Domain.Trim();
        if (string.IsNullOrEmpty(domain)) return BadRequest();

        var takenDomain = await db.ShopDomains.QueryOne(x => x.Domain == domain && x.Verified && x.ShopId != input.ShopId);
        if (takenDomain != null)
        {
            // check if verified on Vercel
            var takenVerified = await domainService.GetDomainInfo(domain);
            if (takenVerified == null) return Problem();
            if (takenVerified.Verified) return Conflict();

            // if now it's not verified then sync with db
            takenDomain.Verified = false;
            var saved = await db.Save();
            if (!saved) return Problem();
        }

        var uid = User.Uid();
        var shopDomain = await db.ShopDomains.QueryOne(
            x => x.Domain == domain && x.ShopId == input.ShopId && x.Shop.OwnerId == uid
        );
        if (shopDomain == null) return NotFound();

        var verified = await domainService.VerifyDomain(domain);
        if (verified)
        {
            shopDomain.Verified = true;
            var saved = await db.Save();
            return saved ? Ok() : Problem();
        }

        var info = await domainService.GetDomainInfo(domain);
        if (info == null) return Accepted(DomainOutput.NoStatus(domain));
        return Accepted(info.Verification == null
            ? DomainOutput.Verified(domain)
            : DomainOutput.NotVerified(domain, info.Verification)
        );
    }
}