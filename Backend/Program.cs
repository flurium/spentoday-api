using Backend.Auth;
using Backend.Config;
using Backend.Services;
using Data;
using Lib;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Load env variables form .env file (in development)
Env.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174") // during dev
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Database
var dbConnectionString = Env.Get("DB_CONNECTION_STRING");
builder.Services.AddDbContext<Db>(options => options.UseNpgsql(dbConnectionString));

// Infrastructure
builder.Services.AddEmail();
builder.Services.AddStorage();

builder.Services.AddScoped<ImageService>();
builder.Services.AddHostedService<BackgroundRunner>();

// Authentication
builder.Services.AddJwt();
builder.Services.AddAuth();

// Logs
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Fatal()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseCustomHeaderProtection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();