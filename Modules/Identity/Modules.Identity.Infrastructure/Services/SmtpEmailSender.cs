using Microsoft.Extensions.Options;
using Modules.Identity.Infrastructure.Abstractions;
using Modules.Identity.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

namespace Modules.Identity.Infrastructure.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;

    public SmtpEmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        using var msg = new MailMessage();
        msg.From = new MailAddress(_opt.FromEmail, _opt.FromName);
        msg.To.Add(new MailAddress(toEmail));
        msg.Subject = subject;
        msg.Body = htmlBody;
        msg.IsBodyHtml = true;

        using var client = new SmtpClient(_opt.SmtpHost, _opt.SmtpPort)
        {
            EnableSsl = _opt.SmtpUseSsl
        };

        if (!string.IsNullOrWhiteSpace(_opt.SmtpUser))
            client.Credentials = new NetworkCredential(_opt.SmtpUser, _opt.SmtpPass);

        // SmtpClient CancellationToken desteklemez; minimal çözüm
        await client.SendMailAsync(msg);
    }
}
