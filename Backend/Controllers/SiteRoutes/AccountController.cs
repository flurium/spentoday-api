using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.UserTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly IStorage storage;
    private readonly ImageService imageService;
    private readonly Db db;

    public AccountController(UserManager<User> userManager, IStorage storage, ImageService imageService, Db db)
    {
        this.userManager = userManager;
        this.storage = storage;
        this.imageService = imageService;
        this.db = db;
    }

    public record OneUser(string Name, string Email, string? ImageUrl);

    [HttpGet("user"), Authorize]
    public async Task<IActionResult> GetUser()
    {
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);

        if (user == null) return NotFound();
        var file = user.GetStorageFile();

        return Ok(new OneUser(Name: user.Name, Email: user.Email, ImageUrl: file != null ? storage.Url(file) : null));
    }

    [HttpPost("image"), Authorize]
    public async Task<IActionResult> SetUserImage(IFormFile file)
    {
        if (!file.IsImage()) return BadRequest();

        var uid = User.Uid();

        var user = await userManager.FindByIdAsync(uid);
        if (user == null) return NotFound();

        var image = user.GetStorageFile();
        if (image != null)
        {
            var deleted = await storage.Delete(image);
            if (!deleted) return Problem();
        }

        var fileId = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadedFile = await storage.Upload(fileId, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        user.ImageProvider = uploadedFile.Provider;
        user.ImageBucket = uploadedFile.Bucket;
        user.ImageKey = uploadedFile.Key;

        var res = await userManager.UpdateAsync(user);
        if (res.Succeeded) return Ok(storage.Url(uploadedFile));

        await storage.Delete(uploadedFile);
        return Problem();
    }

    public record NameInput(string Name);

    [HttpPost("name"), Authorize]
    public async Task<IActionResult> ChangeUserName(NameInput input)
    {
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);
        if (user == null) return NotFound();

        user.Name = input.Name;
        var res = await userManager.UpdateAsync(user);
        if (res.Succeeded) return Ok();

        return Problem();
    }

    public record class PasswordInput(string CurrentPassword, string NewPassword, string ConfirmPassword);

    [HttpPost("password"), Authorize]
    public async Task<IActionResult> ChangePassword(PasswordInput input)
    {
        if (!input.NewPassword.Equals(input.ConfirmPassword)) return BadRequest("confirmPassword");
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);
        if (user == null) return NotFound();

        var res = await userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        user.Version++;
        var changeVersionResult = await userManager.UpdateAsync(user);
        if (res.Succeeded && changeVersionResult.Succeeded) return Ok();

        return Problem();
    }

    public record class DeleteInput(string Email, string Password);

    [HttpPost("delete"), Authorize]
    public async Task<IActionResult> DeleteAccount(DeleteInput input)
    {
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);
        if (user == null) return NotFound();

        var emailCorrect = user.Email == input.Email;
        if (!emailCorrect) BadRequest();

        var passwordCorrect = await userManager.CheckPasswordAsync(user, input.Password);
        if (!passwordCorrect) return BadRequest();

        var userImage = user.GetStorageFile();
        if (userImage != null) await imageService.SafeDelete(userImage);

        var shopLogos = await db.Shops.Where(x => x.OwnerId == uid).Select(x => x.GetStorageFile()).QueryMany();
        await DeleteAllFiles(shopLogos.Where(x => x != null).Select(x => x!));

        var shopBanners = await db.ShopBanners
            .Where(x => x.Shop.OwnerId == uid).Select(x => x.GetStorageFile()).QueryMany();
        await DeleteAllFiles(shopBanners);

        var productImages = await db.ProductImages
            .Where(x => x.Product.Shop.OwnerId == uid).Select(x => x.GetStorageFile()).QueryMany();
        await DeleteAllFiles(productImages);

        var deletion = await userManager.DeleteAsync(user);
        if (!deletion.Succeeded) return Problem();

        Response.Cookies.Delete(RefreshOnly.Cookie);
        return Ok();
    }

    [NonAction]
    public async Task DeleteAllFiles(IEnumerable<StorageFile> files)
    {
        var tasks = new List<Task>(files.Count());
        foreach (var file in files) tasks.Add(imageService.SafeDelete(file));
        await Task.WhenAll(tasks);
    }

    [HttpDelete("image"), Authorize]
    public async Task<IActionResult> DeleteImage()
    {
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);
        if (user == null) return NotFound();

        var image = user.GetStorageFile();

        if (image == null) return NotFound();

        await storage.Delete(image);

        user.ImageKey = null;
        user.ImageProvider = null;
        user.ImageBucket = null;

        var res = await userManager.UpdateAsync(user);

        if (res.Succeeded) return Ok();

        return Problem();
    }
}