using System.Net;
using BuildingBlocks.Web.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web;

public static class GlobalExceptionHandlerExtensions
{
    /// <summary>
    /// Global exception handler that:
    /// - maps FluentValidation.ValidationException (if present) to 422 ValidationProblemDetails
    /// - maps other exceptions to 500 ProblemDetails
    /// Adds: traceId + errorCode.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(err =>
        {
            err.Run(async ctx =>
            {
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                var traceId = ctx.TraceIdentifier;

                if (await TryHandleFluentValidationExceptionAsync(ctx, ex, traceId))
                    return;

                var status = (int)HttpStatusCode.InternalServerError;
                ctx.Response.StatusCode = status;
                ctx.Response.ContentType = "application/problem+json";

                var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();

                await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = status,
                    Title = "Internal Server Error",
                    Detail = env.IsDevelopment() ? ex?.ToString() : "An error occurred.",
                    Type = "https://httpstatuses.com/500",
                    Extensions =
                    {
                        ["traceId"] = traceId,
                        [ProblemDetailsExt.ErrorCodeKey] = "common.unexpected_error"
                    }
                });
            });
        });

        return app;
    }

    private static async Task<bool> TryHandleFluentValidationExceptionAsync(HttpContext ctx, Exception? ex, string traceId)
    {
        // Avoid compile-time dependency on FluentValidation
        if (ex == null) return false;
        var t = ex.GetType();
        if (t.FullName != "FluentValidation.ValidationException") return false;

        var errorsProp = t.GetProperty("Errors");
        var errorsObj = errorsProp?.GetValue(ex);

        var dict = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (errorsObj is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item == null) continue;

                var propName = item.GetType().GetProperty("PropertyName")?.GetValue(item) as string;
                var errorMessage = item.GetType().GetProperty("ErrorMessage")?.GetValue(item) as string;

                propName ??= "";
                errorMessage ??= "Invalid value";

                if (!dict.TryGetValue(propName, out var arr))
                {
                    dict[propName] = [errorMessage];
                }
                else
                {
                    var list = arr.ToList();
                    list.Add(errorMessage);
                    dict[propName] = list.ToArray();
                }
            }
        }

        ctx.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        ctx.Response.ContentType = "application/problem+json";

        await ctx.Response.WriteAsJsonAsync(new ValidationProblemDetails(dict)
        {
            Status = 422,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Type = "https://httpstatuses.com/422",
            Extensions =
            {
                ["traceId"] = traceId,
                [ProblemDetailsExt.ErrorCodeKey] = "common.validation_error"
            }
        });

        return true;
    }
}
