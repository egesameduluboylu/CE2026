using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;

[ApiController]
[Route("api/i18n")]
public class I18nSeedController : ControllerBase
{
    private readonly AuthDbContext _context;

    public I18nSeedController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            // Clear all existing data first
            var allResources = await _context.I18nResources.ToListAsync();
            _context.I18nResources.RemoveRange(allResources);
            await _context.SaveChangesAsync();
            
            await I18nSeedData.SeedAsync(_context);
            return Ok(new { message = "I18n data seeded successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("debug")]
    public async Task<IActionResult> DebugData()
    {
        try
        {
            var resources = await _context.I18nResources
                .Where(r => r.TenantId == null)
                .Select(r => new { r.Key, r.Lang, r.Value })
                .ToListAsync();
            
            return Ok(new { count = resources.Count, data = resources.Take(10) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
