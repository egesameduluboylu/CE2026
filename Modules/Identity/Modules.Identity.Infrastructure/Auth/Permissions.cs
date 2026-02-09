namespace Modules.Identity.Infrastructure.Auth;

public static class Permissions
{
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";

    public static readonly string[] All =
    {
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesWrite,
    };
}
