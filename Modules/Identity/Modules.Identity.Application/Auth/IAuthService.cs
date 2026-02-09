
using Modules.Identity.Contracts.Auth;

namespace Modules.Identity.Application.Auth
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(
            RegisterRequest req,
            AuthAuditContext audit,
            CancellationToken ct = default);

        Task<LoginResult> LoginAsync(
            LoginRequest req,
            AuthAuditContext audit,
            CancellationToken ct = default);

        Task<RefreshResult> RefreshAsync(
            string refreshTokenRaw,
            AuthAuditContext audit,
            CancellationToken ct = default);

        Task LogoutAsync(
            string? refreshTokenRaw,
            AuthAuditContext audit,
            CancellationToken ct = default);
    }
}
