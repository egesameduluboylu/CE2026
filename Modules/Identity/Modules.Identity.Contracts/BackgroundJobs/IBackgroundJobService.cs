namespace Modules.Identity.Contracts.BackgroundJobs;

public interface IBackgroundJobService
{
    // Job management
    Task<BackgroundJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken ct = default);
    Task<BackgroundJobResponse?> GetJobAsync(Guid jobId, CancellationToken ct = default);
    Task<JobListResponse> GetJobsAsync(int page = 1, int pageSize = 20, string? status = null, string? type = null, CancellationToken ct = default);
    Task<bool> UpdateJobAsync(Guid jobId, UpdateJobRequest request, CancellationToken ct = default);
    Task<bool> RetryJobAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> CancelJobAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> DeleteJobAsync(Guid jobId, CancellationToken ct = default);

    // Job execution
    Task<BackgroundJobResponse?> StartJobAsync(Guid jobId, CancellationToken ct = default);
    Task<BackgroundJobResponse?> CompleteJobAsync(Guid jobId, string? result = null, CancellationToken ct = default);
    Task<BackgroundJobResponse?> FailJobAsync(Guid jobId, string errorMessage, string? stackTrace = null, CancellationToken ct = default);

    // Job logging
    Task AddJobLogAsync(Guid jobId, string level, string message, string? data = null, CancellationToken ct = default);
    Task<IReadOnlyList<JobLogResponse>> GetJobLogsAsync(Guid jobId, CancellationToken ct = default);

    // Job statistics
    Task<JobStatisticsResponse> GetJobStatisticsAsync(CancellationToken ct = default);
    Task CleanupExpiredJobsAsync(CancellationToken ct = default);
    Task ProcessScheduledJobsAsync(CancellationToken ct = default);
}

public interface IJobHandler
{
    string JobType { get; }
    Task<bool> HandleAsync(string jobId, string? data, CancellationToken ct = default);
}

public interface IJobQueue
{
    Task EnqueueAsync(string jobType, int priority, string? data = null, string? createdBy = null, CancellationToken ct = default);
    Task<BackgroundJobResponse?> DequeueAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BackgroundJobResponse>> GetPendingJobsAsync(int maxCount = 10, CancellationToken ct = default);
}
