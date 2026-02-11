using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.BackgroundJobs;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.BackgroundJobs;

public sealed class JobQueue : IJobQueue
{
    private readonly AuthDbContext _db;
    private readonly ILogger<JobQueue> _logger;

    public JobQueue(AuthDbContext db, ILogger<JobQueue> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EnqueueAsync(string jobType, int priority, string? data = null, string? createdBy = null, CancellationToken ct = default)
    {
        var job = new BackgroundJob
        {
            Type = jobType,
            Status = JobStatus.Pending,
            Priority = priority,
            Data = data,
            CreatedBy = createdBy
        };

        _db.BackgroundJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Enqueued job {JobId} of type {JobType} with priority {Priority}", job.Id, jobType, priority);
    }

    public async Task<BackgroundJobResponse?> DequeueAsync(CancellationToken ct = default)
    {
        var job = await _db.BackgroundJobs
            .Where(x => x.Status == JobStatus.Pending && 
                       (x.ScheduledAt == null || x.ScheduledAt <= DateTimeOffset.UtcNow))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (job != null)
        {
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Dequeued job {JobId} of type {JobType}", job.Id, job.Type);
        }

        return job != null ? MapToResponse(job) : null;
    }

    public async Task<IReadOnlyList<BackgroundJobResponse>> GetPendingJobsAsync(int maxCount = 10, CancellationToken ct = default)
    {
        var jobs = await _db.BackgroundJobs
            .Where(x => x.Status == JobStatus.Pending && 
                       (x.ScheduledAt == null || x.ScheduledAt <= DateTimeOffset.UtcNow))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .Take(maxCount)
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

        return jobs;
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
