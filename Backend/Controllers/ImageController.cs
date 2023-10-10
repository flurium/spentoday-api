using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("v1/images")]
[ApiController]
public class ImageController : ControllerBase
{
    [HttpGet("{**url}")]
    public IActionResult Image([FromRoute] string url)
    {
        Response.Headers.CacheControl = "public, max-age=31536000";
        return Redirect("https://link.storjshare.io/raw/jxacrqaiskr265rjf7wdytj72bcq/shops/7ceb949e-87d2-48bf-a37f-5482cba019e2.png");
    }
}