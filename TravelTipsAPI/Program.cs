using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TravelTipsAPI.BackgroundServices;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.HereMapServices;
using TravelTipsAPI.Middleware;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.Auth0Services;
using TravelTipsAPI.Services.AzureKeyVaultServices;
using TravelTipsAPI.Services.StripeServices;
using TravelTipsAPI.Services.TravelTipsServices;
using TravelTipsAPI.Services.WikiCommonsServices;
using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContextFactory<TravelTipsContext>(options =>
{
    options.UseLazyLoadingProxies();
    //options.UseSqlServer(builder.Configuration.GetConnectionString("TravelTips"));
    options.UseSqlServer(builder.Configuration.GetConnectionString("TravelTipsLocal"));
});

// Add authentication to the container

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/"; // Must match "iss"
        options.Audience = builder.Configuration["Auth0:Audience"]; // Must match "aud"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
        };
    });

// Add Controllers
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "TravelTips API",
            Version = "v0.8",
            Description = "Updated version of TravelTips API",
        }
    );
    c.CustomSchemaIds(type => type.FullName);
});

// Add Services
builder.Services.AddServices();
builder.Services.AddAuth0Services();
builder.Services.AddHereMapServices();
builder.Services.AddWikiCommonsServices();
builder.Services.AddStripeServices();

// Add Background Services
builder.Services.AddHostedService<HighlightUsageRebuildService>();
builder.Services.AddHostedService<TripBookmarkRebuildService>();
builder.Services.AddHostedService<TripCountRebuildService>();
builder.Services.AddHostedService<UserFollowRebuildService>();

// get the firebase config and register it
var keyVaultUrl = builder.Configuration["AzureKeyVault:Domain"];

var credential = new DefaultAzureCredential();

builder.Services.AddSingleton<IKeyVaultService>(sp =>
{
    return new KeyVaultService(keyVaultUrl!, credential);
});

builder.Services.AddSingleton<FirebaseStorageUploader>(sp =>
{
    var keyVault = sp.GetRequiredService<IKeyVaultService>();
    var jsonSecret = keyVault
        .GetJsonSecretAsync(builder.Configuration["AzureKeyVault:FirebaseKey"]!)
        .Result;
    FirebaseInitializer.InitFirebase(jsonSecret);
    return new FirebaseStorageUploader(jsonSecret);
});

// Add Middleware
builder.Services.AddScoped<EnsureUserMiddleware>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllKnownOrigins",
        policy =>
        {
            policy
                .WithOrigins(Global.URL_PRODUCTION, Global.URL_LOCALHOST)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddSingleton(sp =>
{
    var baseUrl = builder.Configuration["Upstash:Domain"];
    var token = builder.Configuration["Upstash:Token"];
    return new UpstashHttpClient(baseUrl!, token!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllKnownOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();

// use middlewares
app.UseMiddleware<EnsureUserMiddleware>(); // EnsureUserMiddleware should run after authentication to have access to user claims

app.UseAuthorization();

app.MapControllers();

app.Run();
