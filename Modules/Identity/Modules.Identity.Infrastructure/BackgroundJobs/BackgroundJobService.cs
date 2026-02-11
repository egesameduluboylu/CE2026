using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.BackgroundJobs;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using System.Text.Json;

namespace Modules.Identity.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobService : IBackgroundJobService
{
    private readonly AuthDbContext _db;
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly IJobQueue _jobQueue;
    private readonly Dictionary<string, IJobHandler> _handlers;

    public BackgroundJobService(
        AuthDbContext db,
        ILogger<BackgroundJobService> logger,
        IJobQueue jobQueue,
        IEnumerable<IJobHandler> handlers)
    {
        _db = db;
        _logger = logger;
        _jobQueue = jobQueue;
        _handlers = handlers.ToDictionary(h => h.JobType, h => h);
    }

    public async Task<BackgroundJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken ct = default)
    {
        var job = new BackgroundJob
        {
            Type = request.Type,
            Status = JobStatus.Pending,
            Priority = request.Priority,
            Data = request.Data,
            MaxRetries = request.MaxRetries,
            ScheduledAt = request.ScheduledAt,
            ExpiresAt = request.ExpiresAt,
            CreatedBy = request.CreatedBy,
            Metadata = request.Metadata
        };

        _db.BackgroundJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        var response = MapToResponse(job);
        
        _logger.LogInformation("Created background job {JobId} of type {JobType}", job.Id, job.Type);
        
        return response;
    }

    public async Task<BackgroundJobResponse?> GetJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs
            .Include(x => x.Logs)
            .FirstOrDefaultAsync(x => x.Id == jobId, ct);

        return job != null ? MapToResponse(job) : null;
    }

    public async Task<JobListResponse> GetJobsAsync(int page = 1, int pageSize = 20, string? status = null, string? type = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.BackgroundJobs.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(x => x.Type == type);

        var total = await query.CountAsync(ct);
        var pendingCount = await query.CountAsync(x => x.Status == JobStatus.Pending, ct);
        var processingCount = await query.CountAsync(x => x.Status == JobStatus.Processing, ct);
        var failedCount = await query.CountAsync(x => x.Status == JobStatus.Failed, ct);

        var jobs = await query
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BackgroundJobResponse(
                x.Id,
                x.Type,
                x.Status,
                x.Priority,
                x.Data,
                x.Result,
                x.ErrorMessage,
                x.RetryCount,
                x.MaxRetries,
                x.CreatedAt,
                x.StartedAt,
                x.CompletedAt,
                x.ScheduledAt,
                x.ExpiresAt,
                x.CreatedBy,
                x.Metadata
            ))
            .ToListAsync(ct);

        return new JobListResponse(jobs, total, pendingCount, processingCount, failedCount);
    }

    public async Task<bool> UpdateJobAsync(Guid jobId, UpdateJobRequest request, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null) return false;

        job.Status = request.Status;
        job.Result = request.Result;
        job.ErrorMessage = request.ErrorMessage;
        job.StackTrace = request.StackTrace;

        if (request.Status == JobStatus.Completed)
            job.CompletedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated background job {JobId} to status {Status}", jobId, request.Status);
        return true;
    }

    public async Task<bool> RetryJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null || job.RetryCount >= job.MaxRetries) return false;

        job.Status = JobStatus.Pending;
        job.ErrorMessage = null;
        job.StackTrace = null;
        job.RetryCount++;
        job.StartedAt = null;
        job.CompletedAt = null;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Retrying background job {JobId} (attempt {RetryCount}/{MaxRetries})", 
            jobId, job.RetryCount, job.MaxRetries);
        
        return true;
    }

    public async Task<bool> CancelJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null || job.Status == JobStatus.Completed) return false;

        job.Status = JobStatus.Cancelled;
        job.CompletedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled background job {JobId}", jobId);
        return true;
    }

    public async Task<bool> DeleteJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null) return false;

        _db.BackgroundJobs.Remove(job);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted background job {JobId}", jobId);
        return true;
    }

    public async Task<BackgroundJobResponse?> StartJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null || job.Status != JobStatus.Pending) return null;

        job.Status = JobStatus.Processing;
        job.StartedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Starting background job {JobId} of type {JobType}", jobId, job.Type);

        // Execute job in background
        _ = Task.Run(async () => await ExecuteJobAsync(jobId, ct));

        return MapToResponse(job);
    }

    public async Task<BackgroundJobResponse?> CompleteJobAsync(Guid jobId, string? result = null, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null) return null;

        job.Status = JobStatus.Completed;
        job.Result = result;
        job.CompletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Completed background job {JobId}", jobId);
        return MapToResponse(job);
    }

    public async Task<BackgroundJobResponse?> FailJobAsync(Guid jobId, string errorMessage, string? stackTrace = null, CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job == null) return null;

        job.Status = JobStatus.Failed;
        job.ErrorMessage = errorMessage;
        job.StackTrace = stackTrace;
        job.CompletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogError("Failed background job {JobId}: {Error}", jobId, errorMessage);
        return MapToResponse(job);
    }

    public async Task AddJobLogAsync(Guid jobId, string level, string message, string? data = null, CancellationToken ct = default)
    {
        var log = new BackgroundJobLog
        {
            JobId = jobId,
            Level = level,
            Message = message,
            Data = data
        };

        _db.BackgroundJobLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<JobLogResponse>> GetJobLogsAsync(Guid jobId, CancellationToken ct = default)
    {
        var logs = await _db.BackgroundJobLogs
            .AsNoTracking()
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new JobLogResponse(
                x.Id,
                x.JobId,
                x.Level,
                x.Message,
                x.CreatedAt,
                x.Data
            ))
            .ToListAsync(ct);

        return logs;
    }

    public async Task<JobStatisticsResponse> GetJobStatisticsAsync(CancellationToken ct = default)
    {
        var jobs = await _db.BackgroundJobs.AsNoTracking().ToListAsync(ct);
        
        var totalJobs = jobs.Count;
        var pendingJobs = jobs.Count(x => x.Status == JobStatus.Pending);
        var processingJobs = jobs.Count(x => x.Status == JobStatus.Processing);
        var completedJobs = jobs.Count(x => x.Status == JobStatus.Completed);
        var failedJobs = jobs.Count(x => x.Status == JobStatus.Failed);
        var cancelledJobs = jobs.Count(x => x.Status == JobStatus.Cancelled);

        var completedJobsWithDuration = jobs
            .Where(x => x.Status == JobStatus.Completed && x.StartedAt.HasValue && x.CompletedAt.HasValue)
            .Select(x => (x.CompletedAt!.Value - x.StartedAt!.Value).TotalMinutes)
            .ToList();

        var averageProcessingTime = completedJobsWithDuration.Any() 
            ? (int)Math.Round(completedJobsWithDuration.Average(), 0)
            : 0;

        var jobsPerHour = totalJobs > 0 
            ? (int)Math.Round(totalJobs / Math.Max(1, (DateTimeOffset.UtcNow - jobs.Min(x => x.CreatedAt)).TotalHours), 0)
            : 0;

        var jobsByType = jobs
            .GroupBy(x => x.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        var jobsByStatus = jobs
            .GroupBy(x => x.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return new JobStatisticsResponse(
            totalJobs,
            pendingJobs,
            processingJobs,
            completedJobs,
            failedJobs,
            cancelledJobs,
            averageProcessingTime,
            jobsPerHour,
            jobsByType,
            jobsByStatus
        );
    }

    public async Task CleanupExpiredJobsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredJobs = await _db.BackgroundJobs
            .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt.Value < now)
            .ToListAsync(ct);

        if (expiredJobs.Any())
        {
            _db.BackgroundJobs.RemoveRange(expiredJobs);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Cleaned up {Count} expired background jobs", expiredJobs.Count);
        }
    }

    public async Task ProcessScheduledJobsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var scheduledJobs = await _db.BackgroundJobs
            .Where(x => x.Status == JobStatus.Pending && x.ScheduledAt.HasValue && x.ScheduledAt.Value <= now)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.ScheduledAt)
            .ToListAsync(ct);

        foreach (var job in scheduledJobs)
        {
            await StartJobAsync(job.Id, ct);
        }

        if (scheduledJobs.Any())
        {
            _logger.LogInformation("Processed {Count} scheduled background jobs", scheduledJobs.Count);
        }
    }

    private async Task ExecuteJobAsync(Guid jobId, CancellationToken ct)
    {
        try
        {
            var job = await _db.BackgroundJobs.FirstOrDefaultAsync(x => x.Id == jobId, ct);
            if (job == null) return;

            await AddJobLogAsync(jobId, "Info", "Starting job execution", null, ct);

            if (_handlers.TryGetValue(job.Type, out var handler))
            {
                var success = await handler.HandleAsync(jobId.ToString(), job.Data, ct);
                
                if (success)
                {
                    await CompleteJobAsync(jobId, null, ct);
                    await AddJobLogAsync(jobId, "Info", "Job completed successfully", null, ct);
                }
                else
                {
                    await FailJobAsync(jobId, "Job handler returned false", null, ct);
                    await AddJobLogAsync(jobId, "Error", "Job handler returned false", null, ct);
                }
            }
            else
            {
                await FailJobAsync(jobId, $"No handler found for job type: {job.Type}", null, ct);
                await AddJobLogAsync(jobId, "Error", $"No handler found for job type: {job.Type}", null, ct);
            }
        }
        catch (Exception ex)
        {
            await FailJobAsync(jobId, ex.Message, ex.StackTrace, ct);
            await AddJobLogAsync(jobId, "Error", ex.Message, ex.StackTrace, ct);
        }
    }

    private static BackgroundJobResponse MapToResponse(BackgroundJob job)
    {
        return new BackgroundJobResponse(
            job.Id,
            job.Type,
            job.Status,
            job.Priority,
            job.Data,
            job.Result,
            job.ErrorMessage,
            job.RetryCount,
            job.MaxRetries,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.ScheduledAt,
            job.ExpiresAt,
            job.CreatedBy,
            job.Metadata
        );
    }
}
