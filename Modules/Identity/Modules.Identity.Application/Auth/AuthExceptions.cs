namespace Modules.Identity.Application.Auth;

/// <summary>
/// Thrown by IAuthService implementation (Infrastructure). Application handlers catch and map to Result.
/// </summary>
public abstract class AuthException : Exception
{
    public int StatusCode { get; }
    protected AuthException(int statusCode, string message) : base(message) => StatusCode = statusCode;
}

public sealed class UnauthorizedAuthException : AuthException
{
    public UnauthorizedAuthException(string message) : base(401, message) { }
}

public sealed class ConflictAuthException : AuthException
{
    public ConflictAuthException(string message) : base(409, message) { }
}

public sealed class LockedAuthException : AuthException
{
    public DateTimeOffset? LockedUntil { get; }
    public LockedAuthException(string message, DateTimeOffset? lockedUntil) : base(423, message) => LockedUntil = lockedUntil;
}
