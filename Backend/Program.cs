using Backend.Auth;
using Backend.Lib;
using Backend.Lib.Email;
using Backend.Lib.Email.Services;
using Data;
using Microsoft.EntityFrameworkCore;

// Load env variables form .env file (in development)
Env.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<DoubleSubmitCookieFilter>(int.MinValue));

// Swagger Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var dbConnectionString = Env.Get("DB_CONNECTION_STRING");
builder.Services.AddDbContext<Db>(options => options.UseNpgsql(dbConnectionString));

// Jwt token service
JwtSecrets jwtSecrets = new(
    Env.Get("JWT_ISSUER"),
    Env.Get("JWT_AUDIENCE"),
    Env.Get("JWT_SECRET")
);
builder.Services.AddScoped<Jwt>(_ => new(jwtSecrets));

// Email
var resendApiKey = Env.Get("RESEND_API_KEY");
var brevoApiKey = Env.Get("BREVO_API_KEY");
var sendGridApiKey = Env.Get("SENDGRID_API_KEY");
builder.Services.AddSingleton<IEmailSender>(_ => new EmailGod(
    new EmailService(new Resend(resendApiKey), new DayLimiter(100)),
    new EmailService(new Brevo(brevoApiKey), new DayLimiter(300)),
    new EmailService(new SendGrid(sendGridApiKey), new DayLimiter(100))
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();