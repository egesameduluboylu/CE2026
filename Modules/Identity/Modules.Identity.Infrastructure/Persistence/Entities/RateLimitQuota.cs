using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public class RateLimitQuota : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public string EndpointKey { get; set; } = string.Empty; // auth, admin, api

    [Required]
    public long PermitLimit { get; set; }

    [Required]
    public long WindowSeconds { get; set; }

    [Required]
    public long Burst { get; set; }

    public string? UserId { get; set; } // null for tenant-wide quota

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public AppUser? User { get; set; }
}
