using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Identity.Application.Auth;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AuthDbContext _db;
    private readonly PasswordService _pw;
    private readonly TokenService _tokens;
    private readonly IConfiguration _cfg;

    public AuthService(AuthDbContext db, PasswordService pw, TokenService tokens, IConfiguration cfg)
    {
        _db = db;
        _pw = pw;
        _tokens = tokens;
        _cfg = cfg;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest req, AuthAuditContext audit, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(x => x.Email == email, ct))
            throw new ConflictAuthException("Email already registered.");

        var user = new AppUser
        {
            Email = email,
            PasswordHash = _pw.Hash(req.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new RegisterResponse(user.Id.ToString(), user.Email);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest req, AuthAuditContext audit, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null)
            throw new UnauthorizedAuthException("Invalid credentials.");

        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTimeOffset.UtcNow)
            throw new LockedAuthException("Account locked due to failed login attempts.", user.LockoutUntil);

        if (!_pw.Verify(user.PasswordHash, req.Password))
        {
            await ApplyFailedLoginAsync(user, ct);
            throw new UnauthorizedAuthException("Invalid credentials.");
        }

        // Check if user has 2FA enabled (temporarily disabled for testing)
        // var tfaToken = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsEnabled, ct);
        // if (tfaToken != null)
        // {
        //     // Return a special result indicating 2FA is required
        //     return new LoginResult(null, null, true, user.Id); // 2FA required
        // }

        // ba�ar�l� login -> lockout/fail saya�lar�n� temizle ve login bilgilerini güncelle
        if (user.FailedLoginCount != 0 || user.LockoutUntil != null || user.LastFailedLoginAt != null)
        {
            user.FailedLoginCount = 0;
            user.LockoutUntil = null;
            user.LastFailedLoginAt = null;
        }

        // Login bilgilerini güncelle
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.LastLoginIp = audit.IpAddress;
        user.LastLoginUserAgent = audit.UserAgent;
        await _db.SaveChangesAsync(ct);

        // Fetch user permissions for JWT claims
        var permissions = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_db.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionKey)
            .Distinct()
            .ToArrayAsync(ct);

        var access = _tokens.CreateAccessToken(user.Id.ToString(), user.Email, user.IsAdmin, permissions);

        var (rawRefresh, refreshHash) = _tokens.CreateRefreshToken();
        var refreshDays = _cfg.GetValue<int>("Jwt:RefreshTokenDays", 14);

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays),
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync(ct);

        return new LoginResult(access, rawRefresh, false);
    }

    public async Task<RefreshResult> RefreshAsync(string refreshTokenRaw, AuthAuditContext audit, CancellationToken ct = default)
    {
        var incomingHash = _tokens.HashRefreshToken(refreshTokenRaw);

        var token = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);

        if (token == null)
            throw new UnauthorizedAuthException("Invalid refresh token.");

        var now = DateTimeOffset.UtcNow;

        // revoked/expired ise -> reuse/invalid senaryosu gibi t�m aktifleri revoke et
        if (token.RevokedAt != null || token.ExpiresAt <= now)
        {
            await RevokeAllUserRefreshTokens(token.UserId, ct);
            throw new UnauthorizedAuthException("Refresh token reuse detected. Please login again.");
        }

        // rotation
        token.RevokedAt = now;

        var (newRaw, newHash) = _tokens.CreateRefreshToken();
        var refreshDays = _cfg.GetValue<int>("Jwt:RefreshTokenDays", 14);

        var newToken = new RefreshToken
        {
            UserId = token.UserId,
            TokenHash = newHash,
            ExpiresAt = now.AddDays(refreshDays),
        };

        _db.RefreshTokens.Add(newToken);
        token.ReplacedByTokenId = newToken.Id;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await RevokeAllUserRefreshTokens(token.UserId, ct);
            throw new UnauthorizedAuthException("Refresh token reuse detected. Please login again.");
        }

        // Fetch user permissions for JWT claims
        var permissions = await _db.UserRoles
            .Where(ur => ur.UserId == token.UserId)
            .Join(_db.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionKey)
            .Distinct()
            .ToArrayAsync(ct);

        var access = _tokens.CreateAccessToken(token.User.Id.ToString(), token.User.Email, token.User.IsAdmin, permissions);

        // Controller res.NewRefreshTokenRaw bekliyor
        return new RefreshResult(access, newRaw);
    }

    public async Task LogoutAsync(string? refreshTokenRaw, AuthAuditContext audit, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenRaw))
            return;

        var incomingHash = _tokens.HashRefreshToken(refreshTokenRaw);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);
        if (token == null)
            return;

        token.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task ApplyFailedLoginAsync(AppUser user, CancellationToken ct)
    {
        user.FailedLoginCount += 1;
        user.LastFailedLoginAt = DateTimeOffset.UtcNow;

        var maxFailed = _cfg.GetValue<int>("AuthSecurity:MaxFailedLogins", 5);
        var lockMinutes = _cfg.GetValue<int>("AuthSecurity:LockoutMinutes", 10);

        if (user.FailedLoginCount >= maxFailed)
        {
            user.LockoutUntil = DateTimeOffset.UtcNow.AddMinutes(lockMinutes);
            user.FailedLoginCount = 0; // istersen resetleme davran���n� kald�rabiliriz
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task RevokeAllUserRefreshTokens(Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
    }
}
