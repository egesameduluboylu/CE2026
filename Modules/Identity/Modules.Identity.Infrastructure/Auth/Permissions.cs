namespace Modules.Identity.Infrastructure.Auth;

public static class Permissions
{
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";
    public const string TenantsRead = "tenants.read";
    public const string TenantsWrite = "tenants.write";
    public const string BillingRead = "billing.read";
    public const string BillingWrite = "billing.write";
    public const string FlagsRead = "flags.read";
    public const string FlagsWrite = "flags.write";
    public const string AuditRead = "audit.read";
    public const string SessionsRead = "sessions.read";
    public const string SessionsWrite = "sessions.write";
    public const string ApiKeysRead = "api_keys.read";
    public const string ApiKeysWrite = "api_keys.write";
    public const string WebhooksRead = "webhooks.read";
    public const string WebhooksWrite = "webhooks.write";
    public const string HealthRead = "health.read";
    public const string RateLimitRead = "rate_limit.read";
    public const string RateLimitWrite = "rate_limit.write";

    public static readonly string[] All =
    {
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesWrite,
        TenantsRead,
        TenantsWrite,
        BillingRead,
        BillingWrite,
        FlagsRead,
        FlagsWrite,
        AuditRead,
        SessionsRead,
        SessionsWrite,
        ApiKeysRead,
        ApiKeysWrite,
        WebhooksRead,
        WebhooksWrite,
        HealthRead,
        RateLimitRead,
        RateLimitWrite,
    };
}
