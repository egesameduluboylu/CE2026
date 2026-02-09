using System.Text.Json;
using BuildingBlocks.Web.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (HttpErrorCodeException ex)
        {
            await WriteProblem(context, ex.StatusCode, ex.Message, ex.ErrorCode);
        }
        catch (Exception)
        {
            await WriteProblem(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "common.unexpected_error"
            );
        }
    }

    private static async Task WriteProblem(HttpContext ctx, int status, string title, string errorCode)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var pd = new ProblemDetails
        {
            Status = status,
            Title = title,
            Instance = ctx.Request.Path
        };

        pd.Extensions[ProblemDetailsExt.ErrorCodeKey] = errorCode;
        pd.Extensions["traceId"] = ctx.TraceIdentifier;

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(pd, JsonOpts));
    }
}
