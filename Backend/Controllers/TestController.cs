using Backend.Services;
using Data;
using Data.Models;
using Lib;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/test")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly Jwt jwt;
    private readonly IStorage storage;
    private readonly Db db;
    private readonly ImageService imageService;

    public TestController(Jwt jwt, IStorage storage, Db db, ImageService imageService)
    {
        this.jwt = jwt;
        this.storage = storage;
        this.db = db;
        this.imageService = imageService;
    }

    [HttpGet("token")]
    public IActionResult Home()
    {
        return Ok(jwt.Token("1111", 1));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var uid = User.FindFirst(Jwt.Uid)?.Value;
        return Ok(uid);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var key = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var upload = await storage.Upload(key, file.OpenReadStream());
        if (upload == null) return Problem();

        var link = storage.Url(upload);
        return Ok(link);
    }

    [HttpGet("query")]
    public async Task<IActionResult> Query()
    {
        Random rnd = new();

        IQueryable<ProductImage> query = db.Images;

        if (rnd.Next() % 2 == 0)
        {
            query = query.Where(x => x.Id.StartsWith("tscrnmg"));
        }

        var images = await query.QueryMany();

        return Ok(images);
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage()
    {
        var image = await db.Images.QueryOne();
        if (image == null) return NotFound();

        db.Images.Remove(image);
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }

    [HttpDelete("product")]
    public async Task<IActionResult> DeleteProduct()
    {
        var product = await db.Products.QueryOne();
        if (product == null) return NotFound();

        var images = await db.Images.QueryMany(x => x.ProductId == product.Id);
        db.Products.Remove(product);

        var saved = await db.Save();
        if (!saved) return Problem();

        await imageService.SafeDelete(images);
        return Ok();
    }
}