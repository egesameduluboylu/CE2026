namespace Modules.Identity.Infrastructure.Configuration;

public sealed class EmailOptions
{
    public string Provider { get; set; } = "File"; // File | Smtp

    public string FromEmail { get; set; } = "no-reply@localhost";
    public string FromName { get; set; } = "Platform";

    // Frontend link for reset
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";

    // File provider
    public string FilePickupDirectory { get; set; } = "App_Data/mails";

    // SMTP provider
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 25;
    public bool SmtpUseSsl { get; set; } = false;
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
}
