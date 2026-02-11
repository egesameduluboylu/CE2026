using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Tenancy;

public sealed class TenantResolutionMiddleware : IMiddleware
{
    public const string TenantHeader = "X-Tenant-Id";

    private readonly ITenantContext _tenant;

    public TenantResolutionMiddleware(ITenantContext tenant) => _tenant = tenant;

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_tenant is TenantContext tc)
        {
            if (context.Request.Headers.TryGetValue(TenantHeader, out var raw))
            {
                var s = raw.ToString().Trim();
                if (Guid.TryParse(s, out var id))
                    tc.TenantId = id;
            }
        }

        return next(context);
    }
}
