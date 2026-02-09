using BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Identity.Infrastructure.Abstractions;
using Modules.Identity.Infrastructure.Configuration;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Modules.Identity.Application.Auth;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly AuthDbContext _db;
    private readonly IEmailSender _email;
    private readonly EmailOptions _emailOpt;
    private readonly PasswordResetOptions _resetOpt;

    public ForgotPasswordHandler(
        AuthDbContext db,
        IEmailSender email,
        IOptions<EmailOptions> emailOpt,
        IOptions<PasswordResetOptions> resetOpt)
    {
        _db = db;
        _email = email;
        _emailOpt = emailOpt.Value;
        _resetOpt = resetOpt.Value;
    }

    public async Task<Result> Handle(ForgotPasswordCommand cmd, CancellationToken ct)
    {
        var email = (cmd.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Ok(); // enumeration engeli: yine de OK

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email.ToLower() == email, ct);
        if (user is null)
            return Result.Ok(); // enumeration engeli

        var rawToken = GenerateToken();
        var tokenHash = HashToken(rawToken, _resetOpt.TokenPepper);

        var expires = DateTimeOffset.UtcNow.AddMinutes(_resetOpt.TokenMinutes);

        var row = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expires,
            RequestedIp = cmd.Ip,
            UserAgent = cmd.UserAgent
        };

        _db.PasswordResetTokens.Add(row);
        await _db.SaveChangesAsync(ct);

        var link = $"{_emailOpt.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        var subject = "Reset your password";
        var body =
$@"<p>We received a request to reset your password.</p>
<p><a href=""{link}"">Click here to reset your password</a></p>
<p>This link expires in {_resetOpt.TokenMinutes} minutes.</p>
<p>If you did not request this, you can ignore this email.</p>";

        await _email.SendAsync(user.Email, subject, body, ct);

        return Result.Ok();
    }

    private static string GenerateToken()
    {
        var bytes = new byte[48];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string HashToken(string rawToken, string pepper)
    {
        var input = Encoding.UTF8.GetBytes(rawToken + pepper);
        var hash = SHA256.HashData(input);
        return Convert.ToHexString(hash); // 64 chars
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
