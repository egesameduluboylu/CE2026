using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Modules.Identity.Application.Auth;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Configuration;

namespace Host.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly JwtOptions _jwt;
        private readonly AuthCookiesOptions _cookies;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            IAuthService auth,
            IOptions<JwtOptions> jwt,
            IOptions<AuthCookiesOptions> cookies,
            IWebHostEnvironment env)
        {
            _auth = auth;
            _jwt = jwt.Value;
            _cookies = cookies.Value;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _auth.RegisterAsync(req, ct);
                return Ok(ApiResponse.Ok(res, HttpContext.TraceIdentifier));
            }
            catch (ConflictAuthException ex)
            {
                return ApiProblems.Conflict(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _auth.LoginAsync(req, ct);
                SetRefreshCookie(res.RefreshTokenRaw);
                return Ok(ApiResponse.Ok(new LoginResponse(res.AccessToken), HttpContext.TraceIdentifier));
            }
            catch (LockedAuthException ex)
            {
                return ApiProblems.Locked(ex.Message, ex.LockedUntil);
            }
            catch (UnauthorizedAuthException ex)
            {
                return ApiProblems.Unauthorized(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            if (!TryGetRefreshCookie(out var raw))
                return ApiProblems.Unauthorized("Missing refresh cookie.");

            try
            {
                var res = await _auth.RefreshAsync(raw, ct);
                SetRefreshCookie(res.NewRefreshTokenRaw);
                return Ok(ApiResponse.Ok(new RefreshResponse(res.AccessToken), HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAuthException ex)
            {
                // reuse detection dahil
                ClearRefreshCookie();
                return ApiProblems.Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            TryGetRefreshCookie(out var raw);

            await _auth.LogoutAsync(raw, ct);
            ClearRefreshCookie();

            return Ok(ApiResponse.Ok(new { }, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var sub =
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? "";

            var email =
                User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            return Ok(ApiResponse.Ok(new MeResponse(sub, email), HttpContext.TraceIdentifier));
        }

        private bool TryGetRefreshCookie(out string raw)
        {
            var cookieName = _cookies.RefreshCookieName;

            if (!Request.Cookies.TryGetValue(cookieName, out raw!))
                return false;

            return !string.IsNullOrWhiteSpace(raw);
        }

        private CookieOptions RefreshCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays),
                Path = "/api/auth/refresh"
            };
        }

        private void SetRefreshCookie(string refreshToken)
        {
            Response.Cookies.Append(_cookies.RefreshCookieName, refreshToken, RefreshCookieOptions());
        }

        private void ClearRefreshCookie()
        {
            Response.Cookies.Delete(_cookies.RefreshCookieName, new CookieOptions
            {
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth/refresh"
            });
        }
    }
}
