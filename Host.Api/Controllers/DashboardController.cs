using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AuthDbContext _context;

    public DashboardController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted);
            var totalTenants = await _context.Tenants.CountAsync(t => !t.IsDeleted);
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive && !t.IsDeleted);
            var totalRoles = await _context.Roles.CountAsync(r => !r.IsDeleted);
            var totalI18nResources = await _context.I18nResources.CountAsync(r => !r.IsDeleted);

            var recentUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.CreatedAt,
                    u.IsActive,
                    u.LastLoginAt
                })
                .ToListAsync();

            var recentLogins = await _context.Users
                .Where(u => !u.IsDeleted && u.LastLoginAt.HasValue)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(10)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.LastLoginAt,
                    u.FailedLoginCount
                })
                .ToListAsync();

            var tenantStats = await _context.Tenants
                .Where(t => !t.IsDeleted)
                .GroupBy(t => t.IsActive)
                .Select(g => new
                {
                    IsActive = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(new
            {
                users = new
                {
                    total = totalUsers,
                    active = activeUsers,
                    recent = recentUsers
                },
                tenants = new
                {
                    total = totalTenants,
                    active = activeTenants,
                    stats = tenantStats
                },
                roles = new
                {
                    total = totalRoles
                },
                i18n = new
                {
                    total = totalI18nResources
                },
                recentLogins,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity([FromQuery] int days = 7)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            var userRegistrations = await _context.Users
                .Where(u => !u.IsDeleted && u.CreatedAt >= startDate)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            var userLogins = await _context.Users
                .Where(u => !u.IsDeleted && u.LastLoginAt.HasValue && u.LastLoginAt >= startDate)
                .GroupBy(u => u.LastLoginAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            var tenantActivity = await _context.Tenants
                .Where(t => !t.IsDeleted && t.CreatedAt >= startDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            return Ok(new
            {
                userRegistrations,
                userLogins,
                tenantActivity,
                period = $"{days} days",
                startDate,
                endDate = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        try
        {
            // Check database connectivity
            var dbConnection = _context.Database.CanConnect();
            
            // Check essential tables
            var usersCount = _context.Users.Count();
            var tenantsCount = _context.Tenants.Count();
            var rolesCount = _context.Roles.Count();
            var i18nCount = _context.I18nResources.Count();

            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                database = new
                {
                    connected = dbConnection,
                    tables = new
                    {
                        users = usersCount,
                        tenants = tenantsCount,
                        roles = rolesCount,
                        i18n_resources = i18nCount
                    }
                },
                version = "1.0.0",
                uptime = DateTime.UtcNow.Subtract(new DateTime(2023, 1, 1)).TotalDays
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Test database performance
            var dbQueryStart = DateTime.UtcNow;
            var userCount = await _context.Users.CountAsync();
            var dbQueryTime = DateTime.UtcNow.Subtract(dbQueryStart).TotalMilliseconds;

            var i18nQueryStart = DateTime.UtcNow;
            var i18nCount = await _context.I18nResources.CountAsync();
            var i18nQueryTime = DateTime.UtcNow.Subtract(i18nQueryStart).TotalMilliseconds;

            var totalQueryTime = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                database = new
                {
                    userCount,
                    i18nCount,
                    queryTime = new
                    {
                        users_ms = dbQueryTime,
                        i18n_ms = i18nQueryTime,
                        total_ms = totalQueryTime
                    }
                },
                memory = GC.GetTotalMemory(false),
                uptime = DateTime.UtcNow.Subtract(new DateTime(2023, 1, 1)).TotalDays
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
