using BuildingBlocks.Web;
using BuildingBlocks.Web.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Host.Api.Middleware;

public static class ExceptionMiddleware
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(err =>
        {
            err.Run(async ctx =>
            {
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                var traceId = ctx.TraceIdentifier;

                if (ex is ValidationException validationEx)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    ctx.Response.ContentType = "application/problem+json";
                    var errors = validationEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                    await ctx.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
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
                    return;
                }

                var status = (int)HttpStatusCode.InternalServerError;
                ctx.Response.StatusCode = status;
                ctx.Response.ContentType = "application/problem+json";
                await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = status,
                    Title = "Internal Server Error",
                    Detail = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() ? ex?.ToString() : "An error occurred.",
                    Type = "https://httpstatuses.com/500",
                    Extensions = { ["traceId"] = traceId }
                });
            });
        });
        return app;
    }
}
