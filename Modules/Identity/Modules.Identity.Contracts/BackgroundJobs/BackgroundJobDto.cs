using Microsoft.EntityFrameworkCore;

namespace Modules.Identity.Contracts.BackgroundJobs;

/// <summary>
/// Job status constants
/// </summary>
public static class JobStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Job type constants
/// </summary>
public static class JobType
{
    public const string Email = "Email";
    public const string DataProcessing = "DataProcessing";
    public const string ScheduledTask = "ScheduledTask";
    public const string Cleanup = "Cleanup";
    public const string Report = "Report";
}

/// <summary>
/// Job priority constants
/// </summary>
public static class JobPriority
{
    public const int Low = 0;
    public const int Normal = 1;
    public const int High = 2;
    public const int Critical = 3;
}

/// <summary>
/// Job status extensions
/// </summary>
public static class JobStatusExtensions
{
    public static bool IsPending(this string status) => status == JobStatus.Pending;
    public static bool IsProcessing(this string status) => status == JobStatus.Processing;
    public static bool IsCompleted(this string status) => status == JobStatus.Completed;
    public static bool IsFailed(this string status) => status == JobStatus.Failed;
    public static bool IsCancelled(this string status) => status == JobStatus.Cancelled;
    public static bool IsFinished(this string status) => status == JobStatus.Completed || status == JobStatus.Failed || status == JobStatus.Cancelled;
}

/// <summary>
/// Job type extensions
/// </summary>
public static class JobTypeExtensions
{
    public static bool IsEmail(this string type) => type == JobType.Email;
    public static bool IsDataProcessing(this string type) => type == JobType.DataProcessing;
    public static bool IsScheduledTask(this string type) => type == JobType.ScheduledTask;
    public static bool IsCleanup(this string type) => type == JobType.Cleanup;
    public static bool IsReport(this string type) => type == JobType.Report;
}

/// <summary>
/// BackgroundJob extensions
/// </summary>
public static class BackgroundJobExtensions
{
    public static bool IsStarted(this object job) => job != null && job.GetType().GetProperty("Status")?.GetValue(job)?.ToString() == JobStatus.Processing;
    public static bool CanRetry(this object job) 
    {
        if (job == null) return false;
        var retryCount = (int?)job.GetType().GetProperty("RetryCount")?.GetValue(job) ?? 0;
        var maxRetries = (int?)job.GetType().GetProperty("MaxRetries")?.GetValue(job) ?? 0;
        var status = job.GetType().GetProperty("Status")?.GetValue(job)?.ToString();
        return retryCount < maxRetries && status == JobStatus.Failed;
    }
    public static bool IsExpired(this object job) 
    {
        if (job == null) return false;
        var expiresAt = (DateTimeOffset?)job.GetType().GetProperty("ExpiresAt")?.GetValue(job);
        return expiresAt.HasValue && expiresAt < DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Email validation extensions
/// </summary>
public static class EmailValidationExtensions
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Job response for API
/// </summary>
public sealed record BackgroundJobResponse(
    Guid Id,
    string Type,
    string Status,
    int Priority,
    string? Data,
    string? Result,
    string? ErrorMessage,
    int RetryCount,
    int MaxRetries,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? ExpiresAt,
    string? CreatedBy,
    string? Metadata
);

/// <summary>
/// Paged job list response
/// </summary>
public sealed record JobListResponse(
    IReadOnlyList<BackgroundJobResponse> Jobs,
    int Total,
    int PendingCount,
    int ProcessingCount,
    int FailedCount
);

/// <summary>
/// Create job request
/// </summary>
public sealed record CreateJobRequest(
    string Type,
    int Priority,
    string? Data,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? ExpiresAt,
    int MaxRetries,
    string? CreatedBy,
    string? Metadata
);

/// <summary>
/// Update job request
/// </summary>
public sealed record UpdateJobRequest(
    string Status,
    string? Result,
    string? ErrorMessage,
    string? StackTrace
);

/// <summary>
/// Retry job request
/// </summary>
public sealed record RetryJobRequest;

/// <summary>
/// Cancel job request
/// </summary>
public sealed record CancelJobRequest;

/// <summary>
/// Job log response
/// </summary>
public sealed record JobLogResponse(
    Guid Id,
    Guid JobId,
    string Level,
    string Message,
    DateTimeOffset CreatedAt,
    string? Data
);

/// <summary>
/// Job statistics response
/// </summary>
public sealed record JobStatisticsResponse(
    int TotalJobs,
    int PendingJobs,
    int ProcessingJobs,
    int CompletedJobs,
    int FailedJobs,
    int CancelledJobs,
    double AverageProcessingTime,
    int JobsPerHour,
    Dictionary<string, int> JobsByType,
    Dictionary<string, int> JobsByStatus
);
