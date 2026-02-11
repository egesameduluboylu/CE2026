using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.BackgroundJobs;
using System.Text.Json;

namespace Modules.Identity.Infrastructure.BackgroundJobs.Handlers;

public sealed class DataProcessingJobHandler : IJobHandler
{
    private readonly ILogger<DataProcessingJobHandler> _logger;
    private readonly IDataProcessingService _dataProcessingService;

    public string JobType => "DataProcessing";

    public DataProcessingJobHandler(ILogger<DataProcessingJobHandler> logger, IDataProcessingService dataProcessingService)
    {
        _logger = logger;
        _dataProcessingService = dataProcessingService;
    }

    public async Task<bool> HandleAsync(string jobId, string? data, CancellationToken ct = default)
    {
        try
        {
            var processingData = JsonSerializer.Deserialize<DataProcessingJobData>(data ?? "{}");
            
            _logger.LogInformation("Processing data job {JobId} of type {ProcessingType}", jobId, processingData.ProcessingType);

            var success = processingData.ProcessingType switch
            {
                "UserExport" => await ProcessUserExportAsync(jobId, processingData, ct),
                "AnalyticsReport" => await ProcessAnalyticsReportAsync(jobId, processingData, ct),
                "DataCleanup" => await ProcessDataCleanupAsync(jobId, processingData, ct),
                "BackupGeneration" => await ProcessBackupGenerationAsync(jobId, processingData, ct),
                _ => throw new NotSupportedException($"Processing type '{processingData.ProcessingType}' is not supported")
            };

            if (success)
            {
                _logger.LogInformation("Data processing job {JobId} completed successfully", jobId);
            }
            else
            {
                _logger.LogError("Data processing job {JobId} failed", jobId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data job {JobId}", jobId);
            return false;
        }
    }

    private async Task<bool> ProcessUserExportAsync(string jobId, DataProcessingJobData data, CancellationToken ct)
    {
        _logger.LogInformation("Starting user export for date range {StartDate} to {EndDate}", data.StartDate, data.EndDate);
        
        var result = await _dataProcessingService.ExportUsersAsync(data.StartDate, data.EndDate, data.Parameters, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("User export completed. File: {FilePath}", result.FilePath);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessAnalyticsReportAsync(string jobId, DataProcessingJobData data, CancellationToken ct)
    {
        _logger.LogInformation("Generating analytics report for period {StartDate} to {EndDate}", data.StartDate, data.EndDate);
        
        var result = await _dataProcessingService.GenerateAnalyticsReportAsync(data.StartDate, data.EndDate, data.Parameters, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Analytics report generated. Report ID: {ReportId}", result.ReportId);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessDataCleanupAsync(string jobId, DataProcessingJobData data, CancellationToken ct)
    {
        _logger.LogInformation("Starting data cleanup for records older than {Date}", data.StartDate);
        
        var result = await _dataProcessingService.CleanupOldDataAsync(data.StartDate, data.Parameters, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Data cleanup completed. Records deleted: {DeletedCount}", result.DeletedCount);
            return true;
        }
        
        return false;
    }

    private async Task<bool> ProcessBackupGenerationAsync(string jobId, DataProcessingJobData data, CancellationToken ct)
    {
        _logger.LogInformation("Starting backup generation for {BackupType}", data.Parameters.GetValueOrDefault("BackupType", "Full"));
        
        var result = await _dataProcessingService.GenerateBackupAsync(data.Parameters, ct);
        
        if (result.Success)
        {
            _logger.LogInformation("Backup generated successfully. File: {FilePath}", result.FilePath);
            return true;
        }
        
        return false;
    }
}

public sealed record DataProcessingJobData(
    string ProcessingType,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    Dictionary<string, string> Parameters
);

public sealed record DataProcessingResult(
    bool Success,
    string? FilePath = null,
    string? ReportId = null,
    int DeletedCount = 0,
    string? ErrorMessage = null
);

public interface IDataProcessingService
{
    Task<DataProcessingResult> ExportUsersAsync(DateTimeOffset startDate, DateTimeOffset endDate, Dictionary<string, string> parameters, CancellationToken ct);
    Task<DataProcessingResult> GenerateAnalyticsReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Dictionary<string, string> parameters, CancellationToken ct);
    Task<DataProcessingResult> CleanupOldDataAsync(DateTimeOffset cutoffDate, Dictionary<string, string> parameters, CancellationToken ct);
    Task<DataProcessingResult> GenerateBackupAsync(Dictionary<string, string> parameters, CancellationToken ct);
}
