using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Admin;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUsersQuery _q;
    private readonly AuthDbContext _db;

    public AdminUsersController(IAdminUsersQuery q, AuthDbContext db)
    {
        _q = q;
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var data = await _q.GetUsersAsync(search, page, pageSize, ct);
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.read")]
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct = default)
    {
        var data = await _q.GetUserAsync(id, ct);
        if (data is null) return NotFound();
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, [FromServices] PasswordService pw, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return Conflict(ApiResponse.Fail("Email already exists.", HttpContext.TraceIdentifier));

        var user = new AppUser
        {
            Email = email,
            UserName = email,
            PasswordHash = pw.Hash(dto.Password),
            FirstName = dto.FirstName?.Trim(),
            LastName = dto.LastName?.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            IsActive = dto.IsActive,
            IsAdmin = dto.IsAdmin
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { id = user.Id }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid id, [FromBody] LockUserDto dto, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();
        user.LockoutUntil = DateTimeOffset.UtcNow.AddMinutes(dto.Minutes);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { locked = true }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();
        user.LockoutUntil = null;
        user.FailedLoginCount = 0;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { unlocked = true }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/admin")]
    public async Task<IActionResult> SetAdmin(Guid id, [FromBody] SetAdminDto dto, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();
        user.IsAdmin = dto.IsAdmin;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { isAdmin = user.IsAdmin }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/reset-failed")]
    public async Task<IActionResult> ResetFailed(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();
        user.FailedLoginCount = 0;
        user.LastFailedLoginAt = null;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok(new { reset = true }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/revoke-refresh-tokens")]
    public async Task<IActionResult> RevokeRefreshTokens(Guid id, CancellationToken ct = default)
    {
        var data = await _q.RevokeRefreshTokensAsync(id, ct);
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.read")]
    [HttpGet("users/{id:guid}/activities")]
    public async Task<IActionResult> GetUserActivities(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.UserActivities
            .Where(a => a.UserId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Type,
                a.Description,
                a.IpAddress,
                a.UserAgent,
                a.CreatedAt
            });

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(new { items, total, page, pageSize }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.read")]
    [HttpGet("users/{id:guid}/login-history")]
    public async Task<IActionResult> GetUserLoginHistory(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = _db.UserLoginHistories
            .Where(l => l.UserId == id.ToString())
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new
            {
                l.Id,
                l.Success,
                l.FailureReason,
                l.IpAddress,
                l.UserAgent,
                l.Location,
                l.Device,
                l.CreatedAt
            });

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(ApiResponse.Ok(new { items, total, page, pageSize }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPut("users/{id:guid}/profile")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName?.Trim();
        user.LastName = dto.LastName?.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);

        // Log activity
        _db.UserActivities.Add(new UserActivity
        {
            UserId = user.Id,
            Type = "profile_updated",
            Description = "User profile updated by admin",
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        });
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.Ok(new { user.Id, user.FirstName, user.LastName, user.PhoneNumber, user.IsActive }, HttpContext.TraceIdentifier));
    }

    [RequirePermission("users.write")]
    [HttpPost("users/{id:guid}/impersonate")]
    public async Task<IActionResult> ImpersonateUser(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();
        if (!user.IsActive) return BadRequest(ApiResponse.Fail("Cannot impersonate inactive user.", HttpContext.TraceIdentifier));

        // Log impersonation
        _db.UserActivities.Add(new UserActivity
        {
            UserId = user.Id,
            Type = "impersonation_started",
            Description = $"Admin {User.Identity?.Name} started impersonating user {user.Email}",
            IpAddress = GetClientIpAddress(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        });
        await _db.SaveChangesAsync(ct);

        // TODO: Generate impersonation token or set session
        return Ok(ApiResponse.Ok(new { message = "Impersonation started", user = new { user.Id, user.Email, user.FullName } }, HttpContext.TraceIdentifier));
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

public record CreateUserDto(string Email, string Password, string? FirstName = null, string? LastName = null, string? PhoneNumber = null, bool IsActive = true, bool IsAdmin = false);
public record LockUserDto(int Minutes = 30);
public record SetAdminDto(bool IsAdmin);
public record UpdateProfileDto(string? FirstName = null, string? LastName = null, string? PhoneNumber = null, bool IsActive = true);
