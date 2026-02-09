using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Auth;
using Identity.Infrastructure.Auth;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection missing.");

        services.AddDbContext<AuthDbContext>(opt =>
        {
            opt.UseSqlServer(cs, sql =>
            {
                sql.EnableRetryOnFailure(5);
                sql.CommandTimeout(30);
            });
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<PasswordService>();
        services.AddScoped<TokenService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key missing.")
            .ValidateOnStart();

        services.AddOptions<AuthCookiesOptions>()
            .Bind(configuration.GetSection("AuthCookies"))
            .ValidateOnStart();

        services.AddOptions<AuthSecurityOptions>()
            .Bind(configuration.GetSection("AuthSecurity"))
            .ValidateOnStart();

        return services;
    }
}
