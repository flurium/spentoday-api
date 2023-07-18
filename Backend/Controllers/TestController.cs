using Backend.Auth;
using Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly Jwt jwt;

        public TestController(Jwt jwt)
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

        [HttpPost("upload")]
        public IActionResult Upload(IEnumerable<IFormFile> files)
        {
            return Ok();
        }
    }
}