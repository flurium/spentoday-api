﻿using Data;
using Data.Models.UserTables;
using Lib.Email;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.SiteRoutes;

[Route("v1/site")]
[ApiController]
public class QuestionController : ControllerBase
{
    private readonly Db db;
    private readonly IEmailSender emailSender;

    public QuestionController(Db db, IEmailSender email)
    {
        this.db = db;
        this.emailSender = email;
    }

    public record QuestionInput(string Email, string Content);

    [HttpPost("questions")]
    public async Task<IActionResult> SendQuestion(QuestionInput input)
    {
        var email = input.Email.Trim();
        if (string.IsNullOrEmpty(email)) return BadRequest();

        var content = input.Content.Trim();
        if (string.IsNullOrEmpty(content)) return BadRequest();

        var question = new Question(email, content);
        await db.Questions.AddAsync(question);
        var saved = await db.Save();
        if (!saved) return Problem();

        var text = $"Email: {email}\nId:{question.Id}\nContent:{content}";
        await emailSender.Send(email, "Spentoday", new List<string> { "roman@flurium.com" }, "Question on Spentoday", text, text);

        return Ok();
    }
}