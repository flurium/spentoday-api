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

    public TestController(Jwt jwt, IStorage storage, Db db)
    {
        this.jwt = jwt;
        this.storage = storage;
        this.db = db;
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

        var upload = await storage.Upload("shops", key, file.OpenReadStream());
        if (upload == null) return Problem();

        var link = storage.Url(upload);
        return Ok(link);
    }

    [HttpDelete("upload")]
    public async Task<IActionResult> DeleteUpload(string key)
    {
        var delete = await storage.Delete("shops", key);
        return delete ? Ok() : Problem();
    }

    [HttpGet("query")]
    public async Task<IActionResult> Query()
    {
        Random rnd = new();

        IQueryable<ProductImage> query = db.Images;

        if (rnd.Next() % 2 == 0)
        {
            query = query.Where(x => x.Url.StartsWith("tscrnmg"));
        }

        var images = await query.QueryMany();

        return Ok(images);
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage()
    {
        var image = await db.Images.QueryOne(x => x.Url.StartsWith("https://l"));
        if (image == null) return NotFound();

        db.Images.Remove(image);
        var saved = await db.Save();
        return saved ? Ok() : Problem();
    }
}