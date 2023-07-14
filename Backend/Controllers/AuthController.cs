using Backend.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Home()
        {
            return Ok("hello");
        }

        [HttpGet("/me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(User.FindFirst(Jwt.Uid));
        }
    }
}