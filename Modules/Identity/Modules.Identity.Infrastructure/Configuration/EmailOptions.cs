namespace Modules.Identity.Infrastructure.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class EmailOptions
{
    [Required]
    [MinLength(2)]
    public string Provider { get; set; } = "File"; // File | Smtp

    [Required]
    [MinLength(3)]
    public string FromEmail { get; set; } = "no-reply@localhost";

    [Required]
    [MinLength(2)]
    public string FromName { get; set; } = "Platform";

    // Frontend link for reset
    [Required]
    [MinLength(8)]
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";

    // File provider
    [Required]
    [MinLength(1)]
    public string FilePickupDirectory { get; set; } = "App_Data/mails";

    // SMTP provider
    [Required]
    [MinLength(1)]
    public string SmtpHost { get; set; } = "localhost";

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 25;
    public bool SmtpUseSsl { get; set; } = false;
    public string? SmtpUser { get; set; }
    public string? SmtpPass { get; set; }
}
