using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application.Auth;
using Modules.Identity.Application.Behaviors;
using Modules.Identity.Infrastructure.Auth;
using System.Reflection;

namespace Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}