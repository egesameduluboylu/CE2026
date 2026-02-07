using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Contracts.Auth
{
    public record RegisterResponse(Guid Id, string Email);
    public record LoginResponse(string AccessToken);

    public record RefreshResponse(string AccessToken);
    public record MeResponse(string UserId, string? Email);
}
