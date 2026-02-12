using BuildingBlocks.Abstractions.Domain;
using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class I18nResource : BaseEntity
{
    public Guid? TenantId { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Lang { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public long VersionNo { get; set; }
}
