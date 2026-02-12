using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public sealed class Plan : BaseEntity
{
    public string Name { get; set; } = ""; // Basic | Pro | Enterprise etc.
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = new();
    public int MaxUsers { get; set; }
    public int MaxTenants { get; set; }
    public int ApiCallsPerMinute { get; set; }
    public int WebhooksCount { get; set; }
    public bool IsActive { get; set; } = true;
}
