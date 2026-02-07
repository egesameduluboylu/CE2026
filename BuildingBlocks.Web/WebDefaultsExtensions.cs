using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Web
{
    public static class WebDefaultsExtensions
    {
        public static IServiceCollection AddWebDefaults(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
                };
            });

            return services;
        }

        public static IApplicationBuilder UseWebDefaults(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}
