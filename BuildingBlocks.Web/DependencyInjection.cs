using BuildingBlocks.Web.Localization;
using BuildingBlocks.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksWeb(
        this IServiceCollection services,
        Action<CompanyLocalizationOptions>? localization = null)
    {
        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddCompanyLocalization(localization);
        return services;
    }

    public static IApplicationBuilder UseBuildingBlocksWeb(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCompanyLocalization();
        return app;
    }
}
