using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace BuildingBlocks.Observability;

public static class ObservabilityExtensions
{
    public static IHostBuilder UseSerilogWithSeq(this IHostBuilder host, string? seqUrl = null)
    {
        return host.UseSerilog((ctx, cfg) =>
        {
            cfg
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", ctx.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(seqUrl))
                cfg.WriteTo.Seq(seqUrl);
        });
    }

    public static IServiceCollection AddOpenTelemetryDefaults(
        this IServiceCollection services,
        string serviceName,
        string? serviceVersion = null)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r =>
            {
                r.AddService(serviceName, serviceVersion: serviceVersion);
            })
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation();
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation();
            });

        return services;
    }
}
