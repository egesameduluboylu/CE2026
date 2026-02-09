namespace Modules.Identity.Contracts.Admin;

public sealed record AdminUserListItem(
    Guid Id,
    string Email,
    DateTimeOffset CreatedAt,
    bool IsAdmin,
    int FailedLoginCount,
    DateTimeOffset? LockoutUntil);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record AdminRefreshTokenItem(
    Guid Id,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    bool Active);

public sealed record AdminUserDetail(
    Guid Id,
    string Email,
    DateTimeOffset CreatedAt,
    bool IsAdmin,
    int FailedLoginCount,
    DateTimeOffset? LockoutUntil,
    DateTimeOffset? LastFailedLoginAt);

public sealed record AdminUserDetailResponse(
    AdminUserDetail User,
    IReadOnlyList<AdminRefreshTokenItem> RefreshTokens);

public sealed record RevokeTokensResponse(int Revoked);
