using Microsoft.EntityFrameworkCore;
using Modules.Identity.Contracts.Admin;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Application.Admin;

namespace Modules.Identity.Infrastructure.Admin;

public sealed class AdminUsersQuery : IAdminUsersQuery
{
    private readonly AuthDbContext _db;
    public AdminUsersQuery(AuthDbContext db) => _db = db;

    public async Task<PagedResult<AdminUserListItem>> GetUsersAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email.ToLower().Contains(term) ||
                                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                                    (u.LastName != null && u.LastName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListItem(
                u.Id, 
                u.Email, 
                u.FirstName, 
                u.LastName, 
                u.FullName, 
                u.PhoneNumber, 
                u.IsActive, 
                u.CreatedAt, 
                u.IsAdmin, 
                u.FailedLoginCount, 
                u.LockoutUntil, 
                u.LastLoginAt
            ))
            .ToListAsync(ct);

        return new PagedResult<AdminUserListItem>(items, total, page, pageSize);
    }

    public async Task<AdminUserDetailResponse?> GetUserAsync(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new AdminUserDetail(
                u.Id, 
                u.Email, 
                u.FirstName, 
                u.LastName, 
                u.FullName, 
                u.PhoneNumber, 
                u.AvatarUrl, 
                u.IsActive, 
                u.CreatedAt, 
                u.IsAdmin, 
                u.FailedLoginCount, 
                u.LockoutUntil, 
                u.LastFailedLoginAt, 
                u.LastLoginAt, 
                u.LastLoginIp, 
                u.LastLoginUserAgent
            ))
            .FirstOrDefaultAsync(ct);

        if (user is null) return null;

        var now = DateTimeOffset.UtcNow;
        var tokens = await _db.RefreshTokens.AsNoTracking()
            .Where(t => t.UserId == id)
            .Select(t => new AdminRefreshTokenItem(
                t.Id, t.ExpiresAt, t.RevokedAt,
                t.RevokedAt == null && t.ExpiresAt > now
            ))
            .ToListAsync(ct);

        return new AdminUserDetailResponse(user, tokens);
    }

    public async Task<RevokeTokensResponse> RevokeRefreshTokensAsync(Guid id, CancellationToken ct)
    {
        var userIdStr = id.ToString();
        var now = DateTimeOffset.UtcNow;

        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == id && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.RevokedAt = now;

        await _db.SaveChangesAsync(ct);
        return new RevokeTokensResponse(tokens.Count);
    }
}
