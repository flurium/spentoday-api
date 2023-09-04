using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/dashboard")]
[ApiController]
public class PageController : ControllerBase
{
    private readonly Db db;

    public PageController(Db db)
    {
        this.db = db;
    }

    public record PagesOutput(string Slug, string Title, DateTime UpdatedAt);

    [HttpGet("{shopId}/pages")]
    public async Task<IActionResult> Pages(string shopId)
    {
        var pages = await db.InfoPages
            .Where(x => x.ShopId == shopId)
            .Select(x => new PagesOutput(x.Slug, x.Title, x.UpdatedAt))
            .QueryMany();

        return Ok(pages);
    }

    [NonAction]
    public bool IsSlugValid(string slug)
    {
        slug = slug.ToLower().Trim('-');
        for (int i = 0; i < slug.Length; ++i)
        {
            char c = slug[i];
            if (!(char.IsLetter(c) || char.IsDigit(c) || c == '-')) return false;
        }
        return true;
    }

    public record NewPageInput(string Slug);

    /// <returns>
    /// 400 - slug is invalid
    /// 401 - unauthorized
    /// 404 - such show for this user isn't found
    /// 409 - slug is taken
    /// 500 - unexpected error or can't save in database
    /// 200 - created
    /// </returns>
    [HttpPost("{shopId}/page")]
    [Authorize]
    public async Task<IActionResult> NewPage([FromRoute] string shopId, [FromBody] NewPageInput input)
    {
        // validate slug
        var isValid = IsSlugValid(input.Slug);
        if (!isValid) return BadRequest();

        var uid = User.Uid();

        if (await PlanLimits.ReachedPagesLimit(db, uid)) return Forbid();

        var ownShop = await db.Shops.Have(x => x.OwnerId == uid && x.Id == shopId);
        if (!ownShop) return NotFound();

        var slugTaken = await db.InfoPages.Have(x => x.ShopId == shopId && x.Slug == input.Slug);
        if (slugTaken) return Conflict();

        var newInfoPage = new InfoPage(input.Slug, shopId);
        await db.InfoPages.AddAsync(newInfoPage);
        var saved = await db.Save();

        return saved ? Ok(newInfoPage) : Problem();
    }

    public record UpdatePageInput(string? Slug, string? Title, string? Description, string? Content);
    public record PageOutput(string Slug, string Title, string Content, string Description);

    /// <response code="401">User is unauthorized.</response>
    /// <response code="404">Page isn't found.</response>
    /// <response code="400">Slug isn't valid.</response>
    /// <response code="409">Slug alread exists.</response>
    /// <response code="500">Unexpected error or can't save to database.</response>
    /// <response code="200">Successfully updated.</response>
    [HttpPatch("{shopId}/page/{slug}")]
    [Authorize]
    public async Task<IActionResult> UpdatePage(
        [FromRoute] string shopId,
        [FromRoute] string slug,
        [FromBody] UpdatePageInput input
    )
    {
        var uid = User.Uid();

        var page = await db.InfoPages
            .QueryOne(x => x.ShopId == shopId && x.Slug == slug && x.Shop.OwnerId == uid);
        if (page == null) return NotFound();

        if (input.Title != null) page.Title = input.Title;
        if (input.Description != null) page.Description = input.Description;
        if (input.Content != null) page.Content = input.Content;

        if (input.Slug != null)
        {
            var slugValid = IsSlugValid(input.Slug);
            if (!slugValid) return BadRequest();

            var slugTaken = await db.InfoPages.Have(x => x.ShopId == shopId && x.Slug == input.Slug);
            if (slugTaken) return Conflict();

            db.InfoPages.Remove(page);

            var newPage = new InfoPage(input.Slug, shopId)
            {
                Title = page.Title,
                Description = page.Description,
                Content = page.Content
            };

            await db.InfoPages.AddAsync(newPage);
        }

        page.UpdatedAt = DateTime.UtcNow;
        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok(new PageOutput(page.Slug, page.Title, page.Content, page.Description));
    }

    [HttpGet("{shopId}/page/{slug}")]
    [Authorize]
    public async Task<IActionResult> Page([FromRoute] string shopId, [FromRoute] string slug)
    {
        var uid = User.Uid();
        if (uid == null) return Unauthorized();
        var page = await db.InfoPages
            .Where(x => x.ShopId == shopId && x.Slug == slug && x.Shop.OwnerId == uid)
            .Select(x => new PageOutput(x.Slug, x.Title, x.Content, x.Description))
            .QueryOne();

        return Ok(page);
    }
}