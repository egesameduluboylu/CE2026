using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Host.Api.Extensions;
using Host.Api.Modules.Identity;
using Host.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Application;
using Modules.Identity.Infrastructure;
using Modules.Identity.Infrastructure.Persistence;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog: JSON console, traceId in properties
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Identity
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddIdentityApplication();

// Health checks (DB)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks().AddSqlServer(connStr!, name: "sqlserver");

// Refresh token cleanup
builder.Services.AddHostedService<RefreshTokenCleanupService>();

// Web defaults
builder.Services.AddWebDefaults();

// Culture
builder.Services.AddBuildingBlocksWeb(localization: o =>
{
    o.DefaultCulture = new("tr-TR");
    o.EnableCookieProvider = true;
    o.EnableQueryStringProvider = false;
});


// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth_login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth_refresh", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/problem+json; charset=utf-8";

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 429,
            Title = "Too Many Requests",
            Detail = "Rate limit exceeded. Please try again later.",
            Type = "https://httpstatuses.com/429"
        }, token);
    };
});

// JWT Auth (config anahtarları sende Jwt:Key, Jwt:Issuer, Jwt:Audience)
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key missing.");
var issuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer missing.");
var audience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience missing.");

//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opt =>
//    {
//        opt.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidateLifetime = true,

//            ValidIssuer = issuer,
//            ValidAudience = audience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//            ClockSkew = TimeSpan.FromSeconds(30)
//        };
//    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Forwarded headers (IIS / reverse proxy)
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownProxies.Add(System.Net.IPAddress.Loopback);
forwardedOptions.KnownProxies.Add(System.Net.IPAddress.IPv6Loopback);

var app = builder.Build();

var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate");
if (autoMigrate)
{
    await app.ApplyMigrationsAsync();
}

var seed = builder.Configuration.GetValue<bool>("Seed:Enabled");
if (seed)
{
    await app.SeedIdentityAsync();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await PermissionSeeder.SeedAsync(db);
}

app.UseForwardedHeaders(forwardedOptions);

app.UseSerilogRequestLogging();

app.UseWebDefaults();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

// Unauthenticated health for load balancers (optional)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions());

app.Run();
