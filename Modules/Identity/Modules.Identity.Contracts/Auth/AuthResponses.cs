namespace Modules.Identity.Contracts.Auth;

public sealed record LoginResult(string? AccessToken, string? RefreshTokenRaw, bool RequiresTwoFactor = false, string? UserId = null);
public sealed record RefreshResult(string AccessToken, string NewRefreshTokenRaw);
public sealed record RegisterResponse(string UserId, string Email);
public sealed record MeResponse(string UserId, string? Email);

public sealed record LoginResponse(string AccessToken);

public sealed record RefreshResponse(string AccessToken);
