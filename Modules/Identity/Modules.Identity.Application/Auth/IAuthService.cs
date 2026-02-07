using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Application.Auth
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
        Task<LoginResult> LoginAsync(LoginRequest req, CancellationToken ct = default);

        /// <summary>
        /// Refresh cookie’den gelen raw refresh token ile yenileme yapar.
        /// Reuse detection tetiklenirse UnauthorizedException fırlatır.
        /// </summary>
        Task<RefreshResult> RefreshAsync(string refreshTokenRaw, CancellationToken ct = default);

        /// <summary>
        /// Logout: refresh token raw varsa revoke + cookie host'ta silinir.
        /// </summary>
        Task LogoutAsync(string? refreshTokenRaw, CancellationToken ct = default);
    }   
}