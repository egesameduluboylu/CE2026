using BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Http;

public static class HttpClientDefaultsExtensions
{
    /// <summary>
    /// Registers HttpClientFactory and a validated options object for global defaults.
    /// Usage: register named/typed clients in modules, but rely on consistent baseline timeout.
    /// </summary>
    public static IServiceCollection AddCompanyHttpClientDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<HttpClientDefaultsOptions>(configuration, "HttpClient");
        services.AddHttpClient();

        services.AddHttpClient("default", (sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<HttpClientDefaultsOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
        });

        var opt = configuration.GetSection("HttpClient").Get<HttpClientDefaultsOptions>() ?? new HttpClientDefaultsOptions();
        if (opt.ApplyToAll)
        {
            services.ConfigureHttpClientDefaults(b =>
            {
                b.ConfigureHttpClient((sp, client) =>
                {
                    var o = sp.GetRequiredService<IOptions<HttpClientDefaultsOptions>>().Value;
                    client.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
                });
            });
        }

        return services;
    }
}
