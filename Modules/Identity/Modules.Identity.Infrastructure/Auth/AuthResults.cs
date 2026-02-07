using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Infrastructure.Auth
{
    public record LoginResult(string AccessToken, string RefreshTokenRaw);
    public record RefreshResult(string AccessToken, string NewRefreshTokenRaw);
}
