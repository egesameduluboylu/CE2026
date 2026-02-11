using BuildingBlocks.Web.Localization;
using BuildingBlocks.Web.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksWeb(
        this IServiceCollection services,
        Action<CompanyLocalizationOptions>? localization = null)
    {
        services.AddCompanyLocalization(localization);
        services.AddTenantResolution();
        return services;
    }

    public static IApplicationBuilder UseBuildingBlocksWeb(this IApplicationBuilder app)
    {
        app.UseCompanyLocalization();
        app.UseTenantResolution();
        return app;
    }
}
