using System.Text;
using Microsoft.Extensions.Options;
using Modules.Identity.Infrastructure.Abstractions;
using Modules.Identity.Infrastructure.Configuration;

namespace Modules.Identity.Infrastructure.Services;

public sealed class FileEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;

    public FileEmailSender(IOptions<EmailOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var dir = _opt.FilePickupDirectory;
        Directory.CreateDirectory(dir);

        var name = $"{DateTimeOffset.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}.html";
        var path = Path.Combine(dir, name);

        var content =
$@"<h3>To: {toEmail}</h3>
<h3>Subject: {subject}</h3>
<hr/>
{htmlBody}";

        await File.WriteAllTextAsync(path, content, Encoding.UTF8, ct);
    }
}
