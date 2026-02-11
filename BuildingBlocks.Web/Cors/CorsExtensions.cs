using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;

namespace BuildingBlocks.Web.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCompanyCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection("Cors"))
            .ValidateDataAnnotations()
            .Validate(o => !o.Enabled || (o.AllowedOrigins?.Length ?? 0) > 0, "Cors:AllowedOrigins must not be empty when Cors:Enabled=true")
            .Validate(o => !o.Enabled || o.AllowedOrigins.All(x => !string.IsNullOrWhiteSpace(x)), "Cors:AllowedOrigins contains empty value")
            .ValidateOnStart();

        var opt = configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();
        if (opt.Enabled)
        {
            services.AddCors(cors =>
            {
                cors.AddPolicy(opt.PolicyName, p =>
                {
                    p.WithOrigins(opt.AllowedOrigins)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
                });
            });
        }

        return services;
    }

    public static IApplicationBuilder UseCompanyCors(this IApplicationBuilder app, IConfiguration configuration)
    {
        var opt = configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();
        if (!opt.Enabled)
            return app;

        return app.UseCors(opt.PolicyName);
    }
}
