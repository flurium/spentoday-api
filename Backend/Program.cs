using Backend.Auth;
using Backend.Config;
using Backend.Lib;
using Backend.Lib.Email;
using Backend.Lib.Email.Services;
using Data;
using Microsoft.EntityFrameworkCore;

// Load env variables form .env file (in development)
Env.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var dbConnectionString = Env.Get("DB_CONNECTION_STRING");
builder.Services.AddDbContext<Db>(options => options.UseNpgsql(dbConnectionString));

// Email
builder.Services.AddEmail();

// Authentication
builder.Services.AddJwt();
builder.Services.AddAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseDoubleSubmitToken();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();