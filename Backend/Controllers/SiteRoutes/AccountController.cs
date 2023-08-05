using Backend.Auth;
using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Data.Models.UserTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Backend.Controllers.SiteRoutes.ProductController;

namespace Backend.Controllers.SiteRoutes
{
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

        [HttpPost("name"), Authorize]
        public async Task<IActionResult> ChangeUserName(string name)
        {
            var uid = User.Uid();
            var user = await userManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            user.Name = name;
            var res = await userManager.UpdateAsync(user);
            if (res.Succeeded) return Ok();

            return Problem();
        }

        public record class PasswordInput(string currentPassword, string newPassword, string confirmPassword);

        [HttpPost("password"), Authorize]
        public async Task<IActionResult> ChangePassword(PasswordInput input)
        {
            if (!input.newPassword.Equals(input.confirmPassword)) return BadRequest("confirmPassword");
            var uid = User.Uid();
            var user = await userManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            var res = await userManager.ChangePasswordAsync(user, input.currentPassword, input.newPassword);
            if (res.Succeeded) return Ok();

            return Problem();
        }

        public record class DeleteInput(string email, string password);

        [HttpPost("delete"), Authorize]
        public async Task<IActionResult> DeleteAccount(DeleteInput input)
        {
            var uid = User.Uid();
            var user = await userManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            if (user.Email.Equals(input.email) && await userManager.CheckPasswordAsync(user, input.password))
            {
                await userManager.DeleteAsync(user);
                Response.Cookies.Delete(RefreshOnly.Cookie);
                return Ok();
            }

            return Problem();
        }
    }
}
