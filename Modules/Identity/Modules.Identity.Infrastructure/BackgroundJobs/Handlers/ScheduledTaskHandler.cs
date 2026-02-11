using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.BackgroundJobs;
using System.Text.Json;

namespace Modules.Identity.Infrastructure.BackgroundJobs.Handlers;

public sealed class ScheduledTaskHandler : IJobHandler
{
    private readonly ILogger<ScheduledTaskHandler> _logger;
    private readonly IScheduledTaskService _scheduledTaskService;

    public string JobType => "ScheduledTask";

    public ScheduledTaskHandler(ILogger<ScheduledTaskHandler> logger, IScheduledTaskService scheduledTaskService)
    {
        _logger = logger;
        _scheduledTaskService = scheduledTaskService;
    }

    public async Task<bool> HandleAsync(string jobId, string? data, CancellationToken ct = default)
    {
        try
        {
            var taskData = JsonSerializer.Deserialize<ScheduledTaskData>(data ?? "{}");
            
            _logger.LogInformation("Processing scheduled task {JobId} of type {TaskType}", jobId, taskData.TaskType);

            var success = taskData.TaskType switch
            {
                "DailyReport" => await ProcessDailyReportAsync(jobId, taskData, ct),
                "WeeklyCleanup" => await ProcessWeeklyCleanupAsync(jobId, taskData, ct),
                "MonthlyBackup" => await ProcessMonthlyBackupAsync(jobId, taskData, ct),
                "UserStatistics" => await ProcessUserStatisticsAsync(jobId, taskData, ct),
                "SystemHealthCheck" => await ProcessSystemHealthCheckAsync(jobId, taskData, ct),
                "NotificationCleanup" => await ProcessNotificationCleanupAsync(jobId, taskData, ct),
                _ => throw new NotSupportedException($"Task type '{taskData.TaskType}' is not supported")
            };

            if (success)
            {
                _logger.LogInformation("Scheduled task {JobId} completed successfully", jobId);
            }
            else
            {
                _logger.LogError("Scheduled task {JobId} failed", jobId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled task {JobId}", jobId);
            return false;
        }
    }

    private async Task<bool> ProcessDailyReportAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Generating daily report for {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.GenerateDailyReportAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Daily report generated. Report ID: {ReportId}", result.ReportId);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessWeeklyCleanupAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Starting weekly cleanup for week of {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.PerformWeeklyCleanupAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Weekly cleanup completed. Items cleaned: {ItemCount}", result.ItemCount);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessMonthlyBackupAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Starting monthly backup for {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.PerformMonthlyBackupAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Monthly backup completed. Backup file: {BackupFile}", result.BackupFile);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessUserStatisticsAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Calculating user statistics for {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.CalculateUserStatisticsAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("User statistics calculated. Total users: {TotalUsers}, Active users: {ActiveUsers}", 
                result.TotalUsers, result.ActiveUsers);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessSystemHealthCheckAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Performing system health check for {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.PerformSystemHealthCheckAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("System health check completed. Status: {Status}, Issues: {IssueCount}", 
                result.Status, result.IssueCount);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessNotificationCleanupAsync(string jobId, ScheduledTaskData data, CancellationToken ct)
    {
        _logger.LogInformation("Cleaning up old notifications for {Date}", data.ScheduledDate);
        
        var result = await _scheduledTaskService.CleanupOldNotificationsAsync(data.ScheduledDate, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Notification cleanup completed. Notifications deleted: {DeletedCount}", result.DeletedCount);
            return true;
        }
        
        return false;
    }
}

public sealed record ScheduledTaskData(
    string TaskType,
    DateTimeOffset ScheduledDate,
    Dictionary<string, string> Parameters
);

public sealed record ScheduledTaskResult(
    bool Success,
    string? ReportId = null,
    int ItemCount = 0,
    string? BackupFile = null,
    int TotalUsers = 0,
    int ActiveUsers = 0,
    string Status = "Healthy",
    int IssueCount = 0,
    int DeletedCount = 0,
    string? ErrorMessage = null
);

public interface IScheduledTaskService
{
    Task<ScheduledTaskResult> GenerateDailyReportAsync(DateTimeOffset date, CancellationToken ct);
    Task<ScheduledTaskResult> PerformWeeklyCleanupAsync(DateTimeOffset date, CancellationToken ct);
    Task<ScheduledTaskResult> PerformMonthlyBackupAsync(DateTimeOffset date, CancellationToken ct);
    Task<ScheduledTaskResult> CalculateUserStatisticsAsync(DateTimeOffset date, CancellationToken ct);
    Task<ScheduledTaskResult> PerformSystemHealthCheckAsync(DateTimeOffset date, CancellationToken ct);
    Task<ScheduledTaskResult> CleanupOldNotificationsAsync(DateTimeOffset date, CancellationToken ct);
}
