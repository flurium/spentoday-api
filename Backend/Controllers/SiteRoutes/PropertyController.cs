using Backend.Auth;
using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/properties")]
[ApiController]
public class PropertyController : ControllerBase
{
    private readonly Db db;

    public PropertyController(Db db)
    {
        this.db = db;
    }

    public record PostPropertyInput(
        string ProductId,
        string Key,
        string Value
    );

    public record struct PostPropertyOutput(string Id, string Key, string Value);

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PropertiesList([FromBody] PostPropertyInput input)
    {
        var uid = User.Uid();

        var productExist = await db.Products.Have(x => x.Id == input.ProductId && x.Shop.OwnerId == uid);
        if (!productExist) return NotFound();

        var property = new Property(input.Key, input.Value, input.ProductId);
        await db.AddAsync(property);

        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok(new PostPropertyOutput(property.Id, property.Key, property.Value));
    }

    public record EditPropertyInput(
        string Id,
        string Key,
        string Value
    );

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Edit([FromBody] EditPropertyInput input)
    {
        var uid = User.Uid();

        var property = await db.Properties.QueryOne(x => x.Id == input.Id && x.Product.Shop.OwnerId == uid);
        if (property == null) return NotFound();

        property.Value = input.Value;
        property.Key = input.Key;

        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok();
    }

    [HttpDelete("{propertyId}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] string propertyId)
    {
        var uid = User.Uid();

        var property = await db.Properties.QueryOne(x => x.Id == propertyId && x.Product.Shop.OwnerId == uid);
        if (property == null) return NotFound();

        db.Remove(property);
        var saved = await db.Save();
        if (!saved) return Problem();

        return Ok();
    }
}