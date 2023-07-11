using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Home()
        {
            return Ok("hello");
        }
    }
}