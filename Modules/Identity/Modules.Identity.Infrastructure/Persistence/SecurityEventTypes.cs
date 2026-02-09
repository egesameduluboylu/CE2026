namespace Modules.Identity.Infrastructure.Persistence;

public static class SecurityEventTypes
{
    public const string LoginSuccess = "LOGIN_SUCCESS";
    public const string LoginFail = "LOGIN_FAIL";
    public const string Lockout = "LOCKOUT";
    public const string RefreshReused = "REFRESH_REUSED";
    public const string RefreshRotated = "REFRESH_ROTATED";
    public const string Logout = "LOGOUT";
}
