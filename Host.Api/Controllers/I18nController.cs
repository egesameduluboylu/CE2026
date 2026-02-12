using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Application.Services;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/i18n")]
public class I18nController : ControllerBase
{
    private readonly II18nService _i18nService;
    private readonly ILogger<I18nController> _logger;

    public I18nController(II18nService i18nService, ILogger<I18nController> logger)
    {
        _i18nService = i18nService;
        _logger = logger;
    }

    [HttpGet("bundle")]
    public async Task<IActionResult> GetBundle([FromQuery] string lang = "en", [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var etag = await _i18nService.GetETagAsync(tenantId, lang);
            
            if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == etag)
            {
                return StatusCode(304);
            }

            var bundle = await _i18nService.GetBundleAsync(tenantId, lang);
            
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "private, max-age=3600";
            
            return Ok(bundle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting I18n bundle for tenant {TenantId}, lang {Lang}", tenantId, lang);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("admin/list")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetList(
        [FromQuery] string? lang = null,
        [FromQuery] string? search = null,
        [FromQuery] string? prefix = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? tenantId = null)
    {
        try
        {
            var result = await _i18nService.GetListAsync(tenantId, lang, search, prefix, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting I18n list for tenant {TenantId}", tenantId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("admin/upsert")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Upsert([FromBody] I18nUpsertDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Key) || string.IsNullOrWhiteSpace(dto.Lang) || string.IsNullOrWhiteSpace(dto.Value))
            {
                return BadRequest("Key, Lang, and Value are required");
            }

            var result = await _i18nService.UpsertAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting I18n resource for key {Key}", dto.Key);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("admin/publish")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Publish([FromQuery] Guid? tenantId = null)
    {
        try
        {
            await _i18nService.InvalidateCacheAsync(tenantId);
            return Ok(new { message = "Cache invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing I18n for tenant {TenantId}", tenantId);
            return StatusCode(500, "Internal server error");
        }
    }
}
