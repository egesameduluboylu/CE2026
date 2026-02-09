using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Identity.Infrastructure.Configuration;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Application.Auth;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly AuthDbContext _db;
    private readonly PasswordService _passwords;
    private readonly PasswordResetOptions _opt;

    public ResetPasswordHandler(AuthDbContext db, PasswordService passwords, IOptions<PasswordResetOptions> opt)
    {
        _db = db;
        _passwords = passwords;
        _opt = opt.Value;
    }

    public async Task<Result> Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var token = (cmd.Token ?? "").Trim();
        if (string.IsNullOrWhiteSpace(token))
            return Result.Fail(ResultError.Unauthorized("Invalid or expired token."));

        if (string.IsNullOrWhiteSpace(cmd.NewPassword) || cmd.NewPassword.Length < 8)
            return Result.Fail(ResultError.Validation("Password must be at least 8 characters."));

        var tokenHash = HashToken(token, _opt.TokenPepper);
        var now = DateTimeOffset.UtcNow;

        var row = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (row is null || row.UsedAt != null || row.ExpiresAt <= now)
            return Result.Fail(ResultError.Unauthorized("Invalid or expired token."));

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == row.UserId, ct);
        if (user is null)
            return Result.Fail(ResultError.Unauthorized("Invalid or expired token."));

        // set new password
        user.PasswordHash = _passwords.Hash(cmd.NewPassword); // metod adın farklıysa değiştir

        // mark token used
        row.UsedAt = now;

        // revoke all active refresh tokens for this user (force logout everywhere)
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var t in activeTokens)
            t.RevokedAt = now;

        await _db.SaveChangesAsync(ct);

        return Result.Ok();
    }

    private static string HashToken(string rawToken, string pepper)
    {
        var input = Encoding.UTF8.GetBytes(rawToken + pepper);
        var hash = SHA256.HashData(input);
        return Convert.ToHexString(hash);
    }
}
