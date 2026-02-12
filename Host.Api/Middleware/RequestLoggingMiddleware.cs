using System.Diagnostics;

namespace Host.Api.Middleware;

public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        
        // Log request start
        _logger.LogInformation("Starting {Method} {Path} from {RemoteIpAddress}", 
            request.Method, 
            request.Path, 
            context.Connection.RemoteIpAddress);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var response = context.Response;
            
            // Log request completion
            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, "Completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds}ms",
                request.Method,
                request.Path,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
