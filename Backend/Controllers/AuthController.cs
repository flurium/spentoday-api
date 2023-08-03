using Backend.Auth;
using Data.Models.UserTables;
using Lib;
using Lib.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Backend.Controllers
{
    [Route("v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Jwt jwt;
        private readonly UserManager<User> userManager;
        private readonly IEmailSender email;

        public AuthController(Jwt jwt, UserManager<User> userManager, IEmailSender email)
        {
            this.jwt = jwt;
            this.userManager = userManager;
            this.email = email;
        }

        [NonAction]
        public void AddAuthCookie(string token)
        {
            Response.Cookies.Append(RefreshOnly.Cookie, token, new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.Now.AddDays(30)
            });
        }

        public record LoginInput(string Email, string Password);

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginInput input)
        {
            var user = await userManager.FindByNameAsync(input.Email.Trim());
            if (user == null) return NotFound();

            var res = await userManager.CheckPasswordAsync(user, input.Password);
            if (!res) return BadRequest();

            AddAuthCookie(jwt.Token(user.Id, user.Version));
            return Ok();
        }

        public record class RegisterInput(string Email, string Name, string Password, string ConfirmPassword);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterInput input)
        {
            if (!input.Email.Contains('@'))
            {
                return BadRequest();
            }
            if (
                input.Password.Length < 6 ||
                !input.Password.Any(char.IsUpper) ||
                !input.Password.Any(char.IsLower) ||
                !input.Password.Any(char.IsNumber)
            )
            {
                return BadRequest();
            }

            if (!input.Password.Equals(input.ConfirmPassword)) return BadRequest("confirmPassword");

            var user = new User(input.Name, input.Email);

            var res = await userManager.CreateAsync(user, input.Password);
            if (!res.Succeeded)
            {
                return Problem(detail: res.Errors.ElementAt(0).Description, statusCode: 500);
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            string baseUrl = Request.Headers["Referer"].ToString();
            //string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string confirmationLink = $"{baseUrl}account/confirm?token={HttpUtility.UrlEncode(token)}&user={user.Email}";
            //string confirmationLink = $"{baseUrl}confirm?token ={Uri.EscapeDataString(token)}&user ={Uri.EscapeDataString(user.Email)}";

            //Console.WriteLine(confirmationLink);

            await email.Send(
                fromEmail: "support@flurium.com",
                fromName: "spentoday",
                toEmails: new List<string>() { input.Email },
                subject: "ConfirmationLink",
                text: $"Go to this link -> {confirmationLink}",
                html: $"<a href={confirmationLink}>Confirmation Link</a>"
            );

            AddAuthCookie(jwt.Token(user.Id, user.Version));
            return Ok();
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] string token, [FromQuery] string user)
        {
            var u = await userManager.FindByEmailAsync(user);

            if (u == null) return NotFound();
            var res = await userManager.ConfirmEmailAsync(u, token);
            if (!res.Succeeded) return Problem();

            return Ok();
        }

        public record EmailInput(string Email);

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword(EmailInput input)
        {
            var user = await userManager.FindByEmailAsync(input.Email);
            if (user == null) return NotFound();
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            string baseUrl = Request.Headers["Referer"].ToString();

            var callback = $"{baseUrl}account/reset?token={HttpUtility.UrlEncode(token)}&user={user.Email}";

            await email.Send(
                fromEmail: "support@flurium.com",
                fromName: "spentoday",
                toEmails: new List<string>() { user.Email },
                subject: "Reset password token",
                text: $"Go to this link -> {callback}",
                html: $"<a href={callback}>Password Reset Link</a>"
            );
            return Ok();
        }

        public record ResetPasswordInput(string email, string token, string password, string confirmPassword);

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordInput input)
        {
            if (!input.password.Equals(input.password)) return BadRequest();

            var user = await userManager.FindByEmailAsync(input.email);
            if (user == null) return NotFound();

            var resetPassResult = await userManager.ResetPasswordAsync(user, input.token, input.password);
            if (!resetPassResult.Succeeded)
            {
                //foreach (var error in resetPassResult.Errors)
                //{
                //    ModelState.TryAddModelError(error.Code, error.Description);
                //}
                return Problem();
            }
            return Ok();
        }

        //Test method. Will be removed later!!!
        //[HttpGet("remove")]
        //public async Task<IActionResult> Remove()
        //{
        //    var user = await userManager.FindByNameAsync("reskator95@gmail.com");

        //    var res = await userManager.DeleteAsync(user);
        //    return Ok();
        //}
    }
}