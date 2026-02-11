using Microsoft.Extensions.Logging;
using Modules.Identity.Contracts.BackgroundJobs;
using System.Text.Json;

namespace Modules.Identity.Infrastructure.BackgroundJobs.Handlers;

public sealed class EmailJobHandler : IJobHandler
{
    private readonly ILogger<EmailJobHandler> _logger;
    private readonly IEmailService _emailService;

    public string JobType => "Email";

    public EmailJobHandler(ILogger<EmailJobHandler> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<bool> HandleAsync(string jobId, string? data, CancellationToken ct = default)
    {
        try
        {
            var emailData = JsonSerializer.Deserialize<EmailJobData>(data ?? "{}");
            
            if (emailData?.To.IsValidEmail() == false)
            {
                _logger.LogError("Invalid email address: {Email}", emailData?.To);
                return false;
            }

            _logger.LogInformation("Processing email job {JobId} for {Email}", jobId, emailData?.To);

            var success = await _emailService.SendEmailAsync(
                emailData?.To ?? "",
                emailData?.Subject ?? "",
                emailData?.Body ?? "",
                emailData?.IsHtml ?? false,
                emailData?.Attachments,
                ct
            );

            if (success)
            {
                _logger.LogInformation("Email sent successfully to {Email}", emailData?.To);
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}", emailData?.To);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email job {JobId}", jobId);
            return false;
        }
    }
}

public sealed record EmailJobData(
    string To,
    string Subject,
    string Body,
    bool IsHtml = false,
    List<string>? Attachments = null
);

public interface IEmailService
{
    Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        List<string>? attachments = null,
        CancellationToken ct = default
    );
}
