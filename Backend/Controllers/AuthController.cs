using Backend.Auth;
using Backend.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Jwt jwt;

        public AuthController(Jwt jwt)
        {
            this.jwt = jwt;
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
    }
}