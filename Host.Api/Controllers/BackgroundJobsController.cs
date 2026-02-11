using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Contracts.BackgroundJobs;
using System.Security.Claims;
using BuildingBlocks.Web;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/background-jobs")]
[Authorize]
public class BackgroundJobsController : ControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;

    public BackgroundJobsController(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value 
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, [FromQuery] string? type = null, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.GetJobsAsync(page, pageSize, status, type, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching jobs.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.GetJobAsync(jobId, ct);
            if (result == null)
                return NotFound(ApiResponse.Fail("Job not found.", HttpContext.TraceIdentifier));

            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var createRequest = request with { CreatedBy = userId };
            var result = await _backgroundJobService.CreateJobAsync(createRequest, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPut("{jobId}")]
    public async Task<IActionResult> UpdateJob(Guid jobId, [FromBody] UpdateJobRequest request, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.UpdateJobAsync(jobId, request, ct);
            if (!result)
                return NotFound(ApiResponse.Fail("Job not found.", HttpContext.TraceIdentifier));

            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while updating job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("{jobId}/retry")]
    public async Task<IActionResult> RetryJob(Guid jobId, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.RetryJobAsync(jobId, ct);
            if (!result)
                return NotFound(ApiResponse.Fail("Job not found or retry limit exceeded.", HttpContext.TraceIdentifier));

            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrying job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("{jobId}/cancel")]
    public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.CancelJobAsync(jobId, ct);
            if (!result)
                return NotFound(ApiResponse.Fail("Job not found or already completed.", HttpContext.TraceIdentifier));

            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while cancelling job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(Guid jobId, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.DeleteJobAsync(jobId, ct);
            if (!result)
                return NotFound(ApiResponse.Fail("Job not found.", HttpContext.TraceIdentifier));

            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while deleting job.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("{jobId}/logs")]
    public async Task<IActionResult> GetJobLogs(Guid jobId, CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.GetJobLogsAsync(jobId, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching job logs.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetJobStatistics(CancellationToken ct = default)
    {
        try
        {
            var result = await _backgroundJobService.GetJobStatisticsAsync(ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while fetching job statistics.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("cleanup-expired")]
    public async Task<IActionResult> CleanupExpiredJobs(CancellationToken ct = default)
    {
        try
        {
            await _backgroundJobService.CleanupExpiredJobsAsync(ct);
            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while cleaning up expired jobs.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("process-scheduled")]
    public async Task<IActionResult> ProcessScheduledJobs(CancellationToken ct = default)
    {
        try
        {
            await _backgroundJobService.ProcessScheduledJobsAsync(ct);
            return Ok(ApiResponse.Ok(new { success = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing scheduled jobs.", HttpContext.TraceIdentifier));
        }
    }

    // Convenience endpoints for common job types
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailJobRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobRequest = new CreateJobRequest(
                JobType.Email,
                JobPriority.Normal,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    To = request.To,
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = request.IsHtml,
                    Attachments = request.Attachments
                }),
                null,
                null,
                3,
                userId,
                null
            );

            var result = await _backgroundJobService.CreateJobAsync(jobRequest, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while queuing email.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("export-data")]
    public async Task<IActionResult> ExportData([FromBody] DataExportRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobRequest = new CreateJobRequest(
                JobType.DataProcessing,
                JobPriority.Normal,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    ProcessingType = "UserExport",
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Parameters = new Dictionary<string, string> { 
                        ["Format"] = request.Format,
                        ["IncludeInactive"] = request.IncludeInactive.ToString()
                    }
                }),
                null,
                null,
                3,
                userId,
                null
            );

            var result = await _backgroundJobService.CreateJobAsync(jobRequest, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while queuing data export.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("generate-report")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobRequest = new CreateJobRequest(
                JobType.ScheduledTask,
                JobPriority.Normal,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    TaskType = "DailyReport",
                    ScheduledDate = DateTimeOffset.UtcNow,
                    Parameters = new Dictionary<string, string> { 
                        ["ReportType"] = request.ReportType,
                        ["IncludeCharts"] = request.IncludeCharts.ToString()
                    }
                }),
                null,
                null,
                3,
                userId,
                null
            );

            var result = await _backgroundJobService.CreateJobAsync(jobRequest, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while queuing report generation.", HttpContext.TraceIdentifier));
        }
    }
}

// Convenience DTOs for common operations
public sealed record EmailJobRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = false,
    List<string>? Attachments = null
);

public sealed record DataExportRequest(
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Format = "CSV",
    bool IncludeInactive = false
);

public sealed record ReportRequest(
    string ReportType,
    bool IncludeCharts = true
);
