using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
            ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
            ctx.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            ctx.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'");

            await next();
        });
    }
}
