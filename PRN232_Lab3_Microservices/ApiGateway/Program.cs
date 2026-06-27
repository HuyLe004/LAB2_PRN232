using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var secret = jwtSection["Secret"];

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? string.Empty));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(jwtSection.GetValue<int>("ClockSkewSeconds", 0))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Ensure gateway forwards Authorization header so downstream services can enforce [Authorize]
app.Use(async (context, next) =>
{
    // If client provided Authorization, YARP will generally forward it,
    // but this guarantees it stays on the request.
    // No-op unless header exists.
    if (context.Request.Headers.TryGetValue("Authorization", out var auth) && !string.IsNullOrWhiteSpace(auth))
    {
        context.Request.Headers["Authorization"] = auth;
    }
    await next();
});

// Forward everything under /api/* to the reverse proxy.
// JWT validation happens in this gateway via JwtBearer middleware.
app.MapReverseProxy();

app.Run();

