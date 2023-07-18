using Lib;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly Jwt jwt;
        private readonly IStorage storage;

        public TestController(Jwt jwt, IStorage storage)
        {
            this.jwt = jwt;
            this.storage = storage;
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

            if (upload != null) return Ok(upload);

            return Problem();
        }

        [HttpGet("uploads")]
        public async Task<IActionResult> Uploads()
        {
            return Ok();
        }
    }
}