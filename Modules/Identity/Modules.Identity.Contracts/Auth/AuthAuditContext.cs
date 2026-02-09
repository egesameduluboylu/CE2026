namespace Modules.Identity.Contracts.Auth;

/// <summary>
/// Optional context from Host (e.g. HTTP request) for security audit logging.
/// </summary>
public record AuthAuditContext(string? IpAddress, string? UserAgent);
