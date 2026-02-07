using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Application;
using Modules.Identity.Infrastructure;
using Modules.Identity.Infrastructure.Configuration;
using System.Text;

namespace Host.Api.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration cfg)
    {
        // Identity DI
        services.AddIdentityInfrastructure(cfg);
        services.AddIdentityApplication();

        // JWT config (typed options already bound in infra)
        var jwt = cfg.GetSection("Jwt").Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt section missing.");

        if (string.IsNullOrWhiteSpace(jwt.Key))
            throw new InvalidOperationException("Jwt:Key missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
