using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Auth;
using BuildingBlocks.Web;
using System.Security.Claims;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
public class TwoFactorAuthController : ControllerBase
{
    private readonly ITwoFactorAuthService _tfaService;

    public TwoFactorAuthController(ITwoFactorAuthService tfaService)
    {
        _tfaService = tfaService;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value 
               ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupTwoFactor(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _tfaService.SetupTwoFactorAsync(userId, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while setting up 2FA.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _tfaService.VerifyTwoFactorAsync(userId, request, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while verifying 2FA.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _tfaService.DisableTwoFactorAsync(userId, request, ct);
            return Ok(ApiResponse.Ok(new { success = result }, HttpContext.TraceIdentifier));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while disabling 2FA.", HttpContext.TraceIdentifier));
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetTwoFactorStatus(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _tfaService.GetTwoFactorStatusAsync(userId, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while getting 2FA status.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("backup-codes/regenerate")]
    public async Task<IActionResult> RegenerateBackupCodes([FromBody] RegenerateBackupCodesRequest request, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _tfaService.RegenerateBackupCodesAsync(userId, ct);
            return Ok(ApiResponse.Ok(result, HttpContext.TraceIdentifier));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred while regenerating backup codes.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("verify-login")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactorForLogin([FromBody] TwoFactorVerificationRequest request, CancellationToken ct)
    {
        try
        {
            // Get user ID from temporary 2FA session (this would be set during login)
            var userId = HttpContext.Session.GetString("TfaUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse.Fail("No 2FA session found.", HttpContext.TraceIdentifier));
            }

            var isValid = await _tfaService.VerifyTwoFactorCodeAsync(userId, request.Code, ct);
            if (!isValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid verification code.", HttpContext.TraceIdentifier));
            }

            // Clear the 2FA session
            HttpContext.Session.Remove("TfaUserId");

            return Ok(ApiResponse.Ok(new { verified = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred during 2FA verification.", HttpContext.TraceIdentifier));
        }
    }

    [HttpPost("backup-code/verify-login")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyBackupCodeForLogin([FromBody] TwoFactorVerificationRequest request, CancellationToken ct)
    {
        try
        {
            // Get user ID from temporary 2FA session
            var userId = HttpContext.Session.GetString("TfaUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse.Fail("No 2FA session found.", HttpContext.TraceIdentifier));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            var isValid = await _tfaService.UseBackupCodeAsync(userId, request.Code, ipAddress, userAgent, ct);
            if (!isValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid backup code.", HttpContext.TraceIdentifier));
            }

            // Clear the 2FA session
            HttpContext.Session.Remove("TfaUserId");

            return Ok(ApiResponse.Ok(new { verified = true }, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An error occurred during backup code verification.", HttpContext.TraceIdentifier));
        }
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
