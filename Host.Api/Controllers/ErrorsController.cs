using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BuildingBlocks.Web;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/errors")]
[Authorize]
public class ErrorsController : ControllerBase
{
    private readonly ILogger<ErrorsController> _logger;

    public ErrorsController(ILogger<ErrorsController> logger)
    {
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value 
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpGet]
    public async Task<IActionResult> GetErrors([FromQuery] string? level = null, [FromQuery] string? source = null, [FromQuery] string? search = null, [FromQuery] string? dateRange = null, [FromQuery] string? severity = null, CancellationToken ct = default)
    {
        try
        {
            // TODO: Implement actual error logging service
            // For now, return mock data
            var mockErrors = new[]
            {
                new { id = "1", level = "error", message = "Database connection failed", source = "Database", timestamp = DateTime.UtcNow.AddMinutes(-5), severity = "high" },
                new { id = "2", level = "warning", message = "API rate limit exceeded", source = "API", timestamp = DateTime.UtcNow.AddMinutes(-10), severity = "medium" },
                new { id = "3", level = "info", message = "User login successful", source = "Auth", timestamp = DateTime.UtcNow.AddMinutes(-15), severity = "low" }
            };

            return Ok(ApiResponse.Ok(mockErrors, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching error logs");
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching error logs.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("clear")]
    public async Task<IActionResult> ClearErrors([FromBody] ClearErrorsRequest request, CancellationToken ct = default)
    {
        try
        {
            // TODO: Implement actual error clearing service
            _logger.LogInformation("Clearing errors older than {DateRange} with level {Level}", request.OlderThan, request.Level);
            
            return Ok(ApiResponse.Ok("Errors cleared successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing error logs");
            return StatusCode(500, ApiResponse.Fail("An error occurred while clearing error logs.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetErrorStatistics(CancellationToken ct = default)
    {
        try
        {
            // TODO: Implement actual error statistics service
            var mockStats = new
            {
                totalErrors = 150,
                criticalErrors = 5,
                errorCount = 25,
                warningCount = 80,
                infoCount = 40,
                errorsByHour = new Dictionary<string, int>
                {
                    ["00:00"] = 5, ["01:00"] = 3, ["02:00"] = 2, ["03:00"] = 8, ["04:00"] = 12
                },
                topErrors = new[]
                {
                    new { message = "Database connection timeout", count = 15, level = "error" },
                    new { message = "API rate limit exceeded", count = 10, level = "warning" }
                }
            };

            return Ok(ApiResponse.Ok(mockStats, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching error statistics");
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching error statistics.", HttpContext.TraceIdentifier));
        }
    }
}

public class ClearErrorsRequest
{
    public string? OlderThan { get; set; }
    public string? Level { get; set; }
    public string? Source { get; set; }
}
