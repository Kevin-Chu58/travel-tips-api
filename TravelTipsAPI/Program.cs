using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.HereMapServices;
using TravelTipsAPI.Middleware;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.Auth0Services;
using TravelTipsAPI.Services.AzureKeyVaultServices;
using TravelTipsAPI.Services.TravelTipsServices;
using TravelTipsAPI.Services.WikiCommonsServices;
using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContextFactory<TravelTipsContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseSqlServer(builder.Configuration.GetConnectionString("TravelTips"));
    //options.UseSqlServer(builder.Configuration.GetConnectionString("TravelTipsLocal"));
});

// Add authentication to the container.

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
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "TravelTips API",
            Version = "v0.2",
            Description = "Updated version of TravelTips API",
        }
    );
});

// Add Services
builder.Services.AddServices();
builder.Services.AddAuth0Services();
builder.Services.AddHereMapServices();
builder.Services.AddWikiCommonsServices();

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
                .WithOrigins(
                    "https://travel-tips-ui-btbndzc9fndhd5fv.westus2-01.azurewebsites.net"
                //"http://localhost:5173"
                )
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

// Use Middleware
app.Use(
    async (context, next) =>
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            // immediately return 200 OK so CORS preflight succeeds
            context.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }

        var ensureUser = context.RequestServices.GetRequiredService<EnsureUserMiddleware>();
        await ensureUser.InvokeAsync(context, next);
    }
);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
