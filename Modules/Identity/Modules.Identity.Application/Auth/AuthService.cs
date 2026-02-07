
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Application.Auth
{
    public class AuthService : IAuthService
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

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var exists = await _db.Users.AnyAsync(x => x.Email == email, ct);
            if (exists) throw new ConflictAuthException("Email already registered.");

            var user = new AppUser
            {
                Email = email,
                PasswordHash = _pw.Hash(req.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            return new RegisterResponse(user.Id, user.Email);
        }

        public async Task<LoginResult> LoginAsync(LoginRequest req, CancellationToken ct = default)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

            // user enumeration yapma
            if (user == null)
                throw new UnauthorizedAuthException("Invalid credentials.");

            // lockout kontrolü
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTimeOffset.UtcNow)
                throw new LockedAuthException("Account locked due to failed login attempts.", user.LockoutUntil);

            // password check
            if (!_pw.Verify(user.PasswordHash, req.Password))
            {
                await ApplyFailedLoginAsync(user, ct);
                throw new UnauthorizedAuthException("Invalid credentials.");
            }

            // başarılı login -> reset
            if (user.FailedLoginCount != 0 || user.LockoutUntil != null || user.LastFailedLoginAt != null)
            {
                user.FailedLoginCount = 0;
                user.LockoutUntil = null;
                user.LastFailedLoginAt = null;
                await _db.SaveChangesAsync(ct);
            }

            var access = _tokens.CreateAccessToken(user.Id, user.Email);

            var (rawRefresh, refreshHash) = _tokens.CreateRefreshToken();
            var refreshDays = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "14");

            var rt = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays),
            };

            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync(ct);

            return new LoginResult(access, rawRefresh);
        }

        public async Task<RefreshResult> RefreshAsync(string refreshTokenRaw, CancellationToken ct = default)
        {
            var incomingHash = _tokens.HashRefreshToken(refreshTokenRaw);

            var token = await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);

            if (token == null)
                throw new UnauthorizedAuthException("Invalid refresh token.");

            // reuse / expired
            var now = DateTimeOffset.UtcNow;

            // reuse detection
            if (token.RevokedAt != null || token.ExpiresAt <= now)
            {
                await RevokeAllUserRefreshTokens(token.UserId, ct);
                throw new UnauthorizedAuthException("Refresh token reuse detected. Please login again.");
            }

            // rotation - revoke current
            token.RevokedAt = now;

            var (newRaw, newHash) = _tokens.CreateRefreshToken();
            var refreshDays = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "14");

            var newToken = new RefreshToken
            {
                Id = Guid.NewGuid(),               // ✅ önemli (replacedby için)
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
                // ✅ Aynı refresh token paralel kullanıldı -> yarış durumu / reuse gibi ele al
                await RevokeAllUserRefreshTokens(token.UserId, ct);
                throw new UnauthorizedAuthException("Refresh token reuse detected. Please login again.");
            }

            var access = _tokens.CreateAccessToken(token.User.Id, token.User.Email);
            return new RefreshResult(access, newRaw);
        }

        public async Task LogoutAsync(string? refreshTokenRaw, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenRaw))
                return;

            var incomingHash = _tokens.HashRefreshToken(refreshTokenRaw);

            var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);
            if (token == null) return;

            token.RevokedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        private async Task ApplyFailedLoginAsync(AppUser user, CancellationToken ct)
        {
            user.FailedLoginCount += 1;
            user.LastFailedLoginAt = DateTimeOffset.UtcNow;

            var maxFailed = int.Parse(_cfg["AuthSecurity:MaxFailedLogins"] ?? "5");
            var lockMinutes = int.Parse(_cfg["AuthSecurity:LockoutMinutes"] ?? "10");

            if (user.FailedLoginCount >= maxFailed)
            {
                user.LockoutUntil = DateTimeOffset.UtcNow.AddMinutes(lockMinutes);
                user.FailedLoginCount = 0;
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task RevokeAllUserRefreshTokens(Guid userId, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var active = await _db.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync(ct);

            foreach (var t in active)
                t.RevokedAt = now;

            await _db.SaveChangesAsync(ct);
        }
    }
}
