using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Web.Localization;

public static class LocalizationExtensions
{
    public static IServiceCollection AddCompanyLocalization(
        this IServiceCollection services,
        Action<CompanyLocalizationOptions>? configure = null)
    {
        var opt = new CompanyLocalizationOptions();
        configure?.Invoke(opt);

        services.AddSingleton(opt);
        services.AddLocalization(o => o.ResourcesPath = opt.ResourcesPath);

        services.Configure<RequestLocalizationOptions>(o =>
        {
            o.SetDefaultCulture(opt.DefaultCulture.Name);
            o.AddSupportedCultures(opt.SupportedCultures.Select(c => c.Name).ToArray());
            o.AddSupportedUICultures(opt.SupportedCultures.Select(c => c.Name).ToArray());

            var providers = new List<IRequestCultureProvider>();

            if (opt.EnableCookieProvider)
                providers.Add(new CookieRequestCultureProvider());

            providers.Add(new AcceptLanguageHeaderRequestCultureProvider());

            if (opt.EnableQueryStringProvider)
                providers.Add(new QueryStringRequestCultureProvider());

            o.RequestCultureProviders = providers;
        });

        return services;
    }

    public static IApplicationBuilder UseCompanyLocalization(this IApplicationBuilder app)
    {
        var loc = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        return app.UseRequestLocalization(loc.Value);
    }
}
