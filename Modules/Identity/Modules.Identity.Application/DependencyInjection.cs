using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Application.Auth;

namespace Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}