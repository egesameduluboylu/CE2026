using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
