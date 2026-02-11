using BuildingBlocks.Abstractions;
using BuildingBlocks.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Identity.Application.Auth;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Configuration;
using Modules.Identity.Infrastructure.Persistence;
using System.Security.Claims;

namespace Host.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly JwtOptions _jwt;
        private readonly AuthCookiesOptions _cookies;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            IMediator mediator,
            IOptions<JwtOptions> jwt,
            IOptions<AuthCookiesOptions> cookies,
            IWebHostEnvironment env)
        {
            _mediator = mediator;
            _jwt = jwt.Value;
            _cookies = cookies.Value;
            _env = env;
        }

        private (string? Ip, string? UserAgent) GetAudit(HttpContext ctx)
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString()
                     ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            var ua = ctx.Request.Headers.UserAgent.FirstOrDefault();

            return (ip, ua);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
        {
            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new RegisterCommand(req.Email, req.Password, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            if (!result.IsSuccess)
                return ToProblem(result);

            return Ok(ApiResponse.Ok(result.Value!, HttpContext.TraceIdentifier));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(req.Email) || 
                string.IsNullOrWhiteSpace(req.Password) ||
                !req.Email.Contains("@"))
            {
                return BadRequest(new { error = "Invalid email or password" });
            }

            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new LoginCommand(req.Email, req.Password, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            if (!result.IsSuccess)
                return ToProblem(result);

            var value = result.Value!;
            
            // Check if 2FA is required
            if (value.RequiresTwoFactor)
            {
                // TODO: Store user ID in session for 2FA verification
                // HttpContext.Session.SetString("TfaUserId", value.UserId!);
                return Ok(ApiResponse.Ok(new { requiresTwoFactor = true, userId = value.UserId }, HttpContext.TraceIdentifier));
            }

            SetRefreshCookie(value.RefreshTokenRaw!);

            return Ok(ApiResponse.Ok(new LoginResponse(value.AccessToken!), HttpContext.TraceIdentifier));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            if (!TryGetRefreshCookie(out var raw))
                return ApiProblems.Unauthorized("Missing refresh cookie.");

            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new RefreshCommand(raw, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            if (!result.IsSuccess)
            {
                ClearRefreshCookie();
                return ToProblem(result);
            }

            var value = result.Value!;
            SetRefreshCookie(value.NewRefreshTokenRaw);

            return Ok(ApiResponse.Ok(new RefreshResponse(value.AccessToken), HttpContext.TraceIdentifier));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            TryGetRefreshCookie(out var raw);

            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new LogoutCommand(raw, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            ClearRefreshCookie();

            if (!result.IsSuccess)
                return ToProblem(result);

            return Ok(ApiResponse.Ok(new { }, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me([FromServices] AuthDbContext db, CancellationToken ct)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var email =
                User.Identity?.Name
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            // admin shortcut
            var isAdmin = await db.Users
                .Where(x => x.Id.ToString() == userId)
                .Select(x => x.IsAdmin)
                .FirstOrDefaultAsync(ct);

            string[] permissions;

            if (isAdmin)
            {
                permissions = Permissions.All;
            }
            else
            {
                permissions = await db.UserRoles
                    .Where(ur => ur.UserId.ToString() == userId)
                    .Join(db.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.PermissionKey)
                    .Distinct()
                    .ToArrayAsync(ct);
            }

            return Ok(ApiResponse.Ok(
                new
                {
                    userId,
                    email,
                    permissions
                },
                HttpContext.TraceIdentifier));
        }

        private IActionResult ToProblem<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return ApiProblems.BadRequest("Unexpected success result.");

            if (result.Error is not { } err)
                return ApiProblems.BadRequest("Unknown error.");

            return MapErrorToProblem(err);
        }

        private IActionResult ToProblem(Result result)
        {
            if (result.IsSuccess)
                return ApiProblems.BadRequest("Unexpected success result.");

            if (result.Error is not { } err)
                return ApiProblems.BadRequest("Unknown error.");

            return MapErrorToProblem(err);
        }

        private IActionResult MapErrorToProblem(ResultError err)
        {
            DateTimeOffset? lockedUntil = null;

            if (err.Extensions is not null
                && err.Extensions.TryGetValue("lockedUntil", out var v)
                && v is DateTimeOffset dto)
            {
                lockedUntil = dto;
            }

            // ResultError.Code: CONFLICT / UNAUTHORIZED / LOCKED / VALIDATION
            return err.Code switch
            {
                "CONFLICT" => ApiProblems.Conflict(err.Message),

                "UNAUTHORIZED" => ApiProblems.Unauthorized(err.Message),

                "LOCKED" => ApiProblems.Locked(err.Message, lockedUntil),

                // ApiProblems taraf�nda 422 helper yoksa BadRequest'e d��elim (compile garanti)
                "VALIDATION" => ApiProblems.BadRequest(err.Message),

                _ => ApiProblems.BadRequest(err.Message)
            };
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
            var isProd = _env.IsProduction();

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = isProd,                    // prod https
                SameSite = isProd ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays),
                Path = "/"                          // ✅ en kritik fix
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
                SameSite = _env.IsProduction() ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/"
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req, CancellationToken ct)
        {
            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new ForgotPasswordCommand(req.Email, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            // Enumeration engeli için her zaman OK dönmek de yeterli,
            // ama Result failure gelirse Problem dönelim (rate limit vs)
            if (!result.IsSuccess)
                return ToProblem(result);

            return Ok(ApiResponse.Ok(new { }, HttpContext.TraceIdentifier));
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest req, CancellationToken ct)
        {
            var (ip, ua) = GetAudit(HttpContext);

            var cmd = new ResetPasswordCommand(req.Token, req.NewPassword, ip, ua);
            var result = await _mediator.Send(cmd, ct);

            if (!result.IsSuccess)
                return ToProblem(result);

            ClearRefreshCookie();

            return Ok(ApiResponse.Ok(new { }, HttpContext.TraceIdentifier));
        }
    }
}
