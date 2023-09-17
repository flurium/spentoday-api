using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.UserTables;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly Db db;
    private readonly UserManager<User> userManager;
    private readonly ImageService imageService;
    private readonly IStorage storage;

    public AccountController(Db db, UserManager<User> userManager, ImageService imageService, IStorage storage)
    {
        this.db = db;
        this.userManager = userManager;
        this.imageService = imageService;
        this.storage = storage;
    }

    public record OneUser(string Name, string? ImageUrl);

    [HttpGet("user"), Authorize]
    public async Task<IActionResult> GetUser()
    {
        var uid = User.Uid();
        var user = await userManager.FindByIdAsync(uid);

        if (user == null) return NotFound();
        var file = user.GetStorageFile();

        return Ok(new OneUser(Name: user.Name, ImageUrl: file != null ? storage.Url(file) : null));
    }

    [HttpPost("image"), Authorize]
    public async Task<IActionResult> SetUserImage(IFormFile file)
    {
        if (!file.IsImage()) return BadRequest();

        var uid = User.Uid();

        var fileId = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadedFile = await storage.Upload(fileId, file.OpenReadStream());
        if (uploadedFile == null) return Problem();

        var user = await userManager.FindByIdAsync(uid);

        if (user == null) return NotFound();

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

        if (user.Email.Equals(input.Email) && await userManager.CheckPasswordAsync(user, input.Password))
        {
            await userManager.DeleteAsync(user);
            Response.Cookies.Delete(RefreshOnly.Cookie);
            return Ok();
        }

        return Problem();
    }
}