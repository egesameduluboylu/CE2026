using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Application.Services;

public interface II18nService
{
    Task<I18nBundleDto> GetBundleAsync(Guid? tenantId, string language, CancellationToken cancellationToken = default);
    Task<I18nListDto> GetListAsync(Guid? tenantId, string? language = null, string? search = null, string? prefix = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<I18nResourceDto> UpsertAsync(I18nUpsertDto dto, CancellationToken cancellationToken = default);
    Task InvalidateCacheAsync(Guid? tenantId, CancellationToken cancellationToken = default);
    Task<string> GetETagAsync(Guid? tenantId, string language, CancellationToken cancellationToken = default);
}

public class I18nService : II18nService
{
    private readonly AuthDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<I18nService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public I18nService(AuthDbContext context, IMemoryCache cache, ILogger<I18nService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<I18nBundleDto> GetBundleAsync(Guid? tenantId, string language, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetBundleCacheKey(tenantId, language);
        
        if (_cache.TryGetValue(cacheKey, out I18nBundleDto? cachedBundle))
        {
            _logger.LogDebug("Bundle found in cache for tenant {TenantId}, lang {Language}", tenantId, language);
            return cachedBundle!;
        }

        var resources = await GetResourcesWithFallbackAsync(tenantId, language, cancellationToken);
        
        var bundle = new I18nBundleDto
        {
            Resources = resources.GroupBy(r => r.Key)
                .ToDictionary(g => g.Key, g => g.First().Value)
        };

        _cache.Set(cacheKey, bundle, CacheDuration);
        
        _logger.LogDebug("Bundle loaded from database for tenant {TenantId}, lang {Language}, count: {Count}", 
            tenantId, language, bundle.Resources.Count);

        return bundle;
    }

    public async Task<I18nListDto> GetListAsync(Guid? tenantId, string? language = null, string? search = null, string? prefix = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.I18nResources.AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(r => r.TenantId == tenantId.Value);
        }
        else
        {
            query = query.Where(r => r.TenantId == null);
        }

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(r => r.Lang == language);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => r.Key.Contains(search) || r.Value.Contains(search));
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            query = query.Where(r => r.Key.StartsWith(prefix));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.Key)
            .ThenBy(r => r.Lang)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new I18nResourceDto
            {
                Id = r.Id,
                TenantId = r.TenantId,
                Key = r.Key,
                Lang = r.Lang,
                Value = r.Value,
                VersionNo = r.VersionNo,
                UpdatedAt = r.UpdatedAt ?? DateTime.UtcNow
            })
            .ToListAsync(cancellationToken);

        return new I18nListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<I18nResourceDto> UpsertAsync(I18nUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _context.I18nResources
            .FirstOrDefaultAsync(r => r.TenantId == dto.TenantId && r.Key == dto.Key && r.Lang == dto.Lang, cancellationToken);

        if (existing == null)
        {
            existing = new I18nResource
            {
                TenantId = dto.TenantId,
                Key = dto.Key,
                Lang = dto.Lang,
                Value = dto.Value,
                VersionNo = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.I18nResources.Add(existing);
        }
        else
        {
            existing.Value = dto.Value;
            existing.VersionNo++;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.I18nResources.Update(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync(dto.TenantId, cancellationToken);

        _logger.LogInformation("Upserted I18n resource: {TenantId}/{Key}/{Lang}", dto.TenantId, dto.Key, dto.Lang);

        return new I18nResourceDto
        {
            Id = existing.Id,
            TenantId = existing.TenantId,
            Key = existing.Key,
            Lang = existing.Lang,
            Value = existing.Value,
            VersionNo = existing.VersionNo,
            UpdatedAt = existing.UpdatedAt ?? DateTime.UtcNow
        };
    }

    public async Task InvalidateCacheAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        var languages = await _context.I18nResources
            .Where(r => r.TenantId == tenantId)
            .Select(r => r.Lang)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var lang in languages)
        {
            var cacheKey = GetBundleCacheKey(tenantId, lang);
            _cache.Remove(cacheKey);
        }

        _logger.LogDebug("Invalidated cache for tenant {TenantId}, languages: {Languages}", tenantId, string.Join(", ", languages));
    }

    public async Task<string> GetETagAsync(Guid? tenantId, string language, CancellationToken cancellationToken = default)
    {
        var resources = await _context.I18nResources
            .Where(r => r.TenantId == tenantId && r.Lang == language)
            .ToListAsync(cancellationToken);

        if (!resources.Any())
        {
            return $"\"{tenantId?.ToString() ?? "null"}-{language}-0-{DateTime.MinValue:yyyyMMddHHmmss}\"";
        }

        var maxVersion = resources.Max(r => r.VersionNo);
        var maxUpdatedAt = resources.Max(r => r.UpdatedAt ?? DateTime.MinValue);

        var etag = $"\"{tenantId?.ToString() ?? "null"}-{language}-{maxVersion}-{maxUpdatedAt:yyyyMMddHHmmss}\"";
        
        return etag;
    }

    private async Task<List<I18nResource>> GetResourcesWithFallbackAsync(Guid? tenantId, string language, CancellationToken cancellationToken)
    {
        var resources = new List<I18nResource>();
        var processedKeys = new HashSet<string>();

        var primaryResources = await _context.I18nResources
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Lang == language)
            .ToListAsync(cancellationToken);

        resources.AddRange(primaryResources);
        foreach (var r in primaryResources)
        {
            processedKeys.Add(r.Key);
        }

        var fallbackLanguage = GetFallbackLanguage(language);
        if (!string.IsNullOrEmpty(fallbackLanguage))
        {
            var fallbackResources = await _context.I18nResources
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && r.Lang == fallbackLanguage && !processedKeys.Contains(r.Key))
                .ToListAsync(cancellationToken);

            resources.AddRange(fallbackResources);
            foreach (var r in fallbackResources)
            {
                processedKeys.Add(r.Key);
                _logger.LogDebug("Used fallback language {Fallback} for key {Key}", fallbackLanguage, r.Key);
            }
        }

        var englishResources = await _context.I18nResources
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Lang == "en" && !processedKeys.Contains(r.Key))
            .ToListAsync(cancellationToken);

        resources.AddRange(englishResources);
        foreach (var r in englishResources)
        {
            _logger.LogDebug("Used English fallback for key {Key}", r.Key);
        }

        return resources;
    }

    private static string GetBundleCacheKey(Guid? tenantId, string language)
    {
        return $"i18n:bundle:{tenantId?.ToString() ?? "null"}:{language}";
    }

    private static string? GetFallbackLanguage(string language)
    {
        if (language.StartsWith("tr-"))
        {
            return "tr";
        }
        
        if (language.Contains("-"))
        {
            return language.Split("-")[0];
        }

        return null;
    }
}
