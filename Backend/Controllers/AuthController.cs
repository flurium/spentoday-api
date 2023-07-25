using Backend.Auth;
using Data.Models.UserTables;
using Lib;
using Lib.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginInput input)
        {
            var user = await userManager.FindByNameAsync(input.Email.Trim());
            if (user == null) return NotFound();

            var res = await userManager.CheckPasswordAsync(user, input.Password);
            if (!res) return BadRequest();

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
            };
            Response.Cookies.Append(RefreshOnly.Cookie, jwt.Token(user.Id, user.Version), cookieOptions);
            return Ok();
        }

        public class LoginInput
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterInput
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

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
                !input.Password.Any(char.IsNumber) ||
                !input.Password.Any(char.IsPunctuation)
            )
            {
                return BadRequest();
            }

            if (!input.Password.Equals(input.ConfirmPassword)) return BadRequest("confirmPassword");

            var user = new User(input.Name, input.Email);

            var res = await userManager.CreateAsync(user, input.Password);
            if (!res.Succeeded)
            {
                return Problem();
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("", "confirmation", new { guid = token, userEmail = user.Email }, Request.Scheme, Request.Host.Value);
            await email.Send(
                fromEmail: "support@flurium.com",
                fromName: "spentoday",
                toEmails: new List<string>() { input.Email },
                subject: "ConfirmationLink",
                text: $"Go to this link:{confirmationLink}",
                html: $"<a href={confirmationLink}>Confirmation Link</a>"
            );

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30)
            };
            Response.Cookies.Append(RefreshOnly.Cookie, jwt.Token(user.Id, user.Version), cookieOptions);

            return Ok();
        }
    }
}