using Backend.Auth;
using Backend.Config;
using Backend.Services;
using Data;
using Lib;
using Microsoft.EntityFrameworkCore;

// Load env variables form .env file (in development)
Env.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
//bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Database
builder.Services.AddDbContext<Db>(options => options.UseNpgsql(Secrets.DB_CONNECTION_STRING));

// Infrastructure
builder.Services.AddEmail();
builder.Services.AddStorage();
builder.Services.AddDomainService();

builder.Services.AddSingleton<BackgroundQueue>();
builder.Services.AddHostedService<BackgroundQueue.HostedService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<CategoryService>();

// Authentication
builder.Services.AddJwt();
builder.Services.AddAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// CORS for origin checking, super important
if (app.Environment.IsProduction()) app.UseDomainCors();

app.UseCors(options =>
{
    options
        .SetIsOriginAllowed(_ => true) // UseDomainCors handle it.
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.UseCustomHeaderProtection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();