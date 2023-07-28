using Backend.Auth;
using Backend.Config;
using Backend.Services;
using Data;
using Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;

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
var dbConnectionString = Env.Get("DB_CONNECTION_STRING");
builder.Services.AddDbContext<Db>(options => options.UseNpgsql(dbConnectionString));

// Infrastructure
builder.Services.AddEmail();
builder.Services.AddStorage();
builder.Services.AddDomainService();

builder.Services.AddSingleton<BackgroundQueue>();
builder.Services.AddHostedService<BackgroundQueue.Runner>();
builder.Services.AddScoped<ImageService>();

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

app.UseCors(options =>
{
    options
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    if (app.Environment.IsProduction())
    {
        options.SetIsOriginAllowed(origin =>
        {
            string hostname = new Uri(origin).Host;

            if (hostname.EndsWith("spentoday.com") || hostname.EndsWith("flurium.com")) return true;

            // maybe change in future
            var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Db>();
            var domainAllowed = db.ShopDomains.Any(x => x.Domain == hostname);
            return domainAllowed;
        });
    }
    else
    {
        options.SetIsOriginAllowed(_ => true);
    }
});

app.UseCustomHeaderProtection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();