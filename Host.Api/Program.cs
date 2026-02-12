using System.Text;
using Serilog;
using Modules.Identity.Infrastructure.Persistence;
using Host.Api.Modules.Identity;
using Host.Api.Middleware;

// Set UTF-8 encoding
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
        .WriteTo.Console();
});

// Identity Module (DB, Auth services, JWT, Authorization, custom Role/Permission system)
builder.Services.AddIdentityModule(builder.Configuration);

// Session for 2FA
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();

// HttpContextAccessor for UserContext
builder.Services.AddHttpContextAccessor();

// Custom Middleware
builder.Services.AddTransient<GlobalExceptionHandler>();
builder.Services.AddTransient<RequestLoggingMiddleware>();

// SignalR
builder.Services.AddSignalR();

// Background Worker - TODO: Uncomment when BackgroundWorkerService is created
//builder.Services.AddHostedService<BackgroundWorkerService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>();

var app = builder.Build();

// Middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// TODO: Add NotificationHub when SignalR is properly configured
// app.MapHub<NotificationHub>("/hub/notifications");
app.MapHealthChecks("/health");

// Seed database
using (var scope = app.Services.CreateScope())
{
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await AuthDbContextSeed.SeedAsync(authDb);
}

app.Run();
