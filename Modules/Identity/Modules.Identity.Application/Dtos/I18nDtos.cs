namespace Modules.Identity.Application.Dtos;

public class I18nBundleDto
{
    public Dictionary<string, string> Resources { get; set; } = new();
}

public class I18nResourceDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public long VersionNo { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class I18nUpsertDto
{
    public Guid? TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class I18nListDto
{
    public List<I18nResourceDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
