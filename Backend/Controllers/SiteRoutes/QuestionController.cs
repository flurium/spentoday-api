using Data;
using Data.Models.UserTables;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site")]
[ApiController]
public class QuestionController : ControllerBase
{
    private readonly Db db;

    public QuestionController(Db db)
    { this.db = db; }

    public record QuestionInput(string Email, string Content);

    [HttpPost("questions")]
    public async Task<IActionResult> SendQuestion(QuestionInput input)
    {
        var email = input.Email.Trim();
        if (string.IsNullOrEmpty(email)) return BadRequest();

        var content = input.Content.Trim();
        if (string.IsNullOrEmpty(content)) return BadRequest();

        await db.Questions.AddAsync(new Question(email, content));
        var saved = await db.Save();

        return saved ? Ok() : Problem();
    }
}