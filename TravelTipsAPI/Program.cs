using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Log to console
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger("Startup");

builder.Configuration.AddEnvironmentVariables();

// First, check the connection string from Azure web service is available
var connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_AZURE_SQL_CONNECTION_STRING");

// If not, fallback to appsettings.json (useful for local dev)
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("TravelTips");
}

// Debug
logger.LogInformation($"Connection String: {connectionString}");

builder.Services.AddDbContext<TravelTipsBasicContext>(options =>
    options.UseSqlServer(connectionString)
);

// Add authentication to the container.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/"; // Must match "iss"
    options.Audience = builder.Configuration["Auth0:Audience"]; // Must match "aud"
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost3000");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
