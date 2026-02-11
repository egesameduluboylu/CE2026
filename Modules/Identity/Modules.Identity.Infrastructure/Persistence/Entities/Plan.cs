using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public sealed class Plan : BaseEntity
{
    public string Name { get; set; } = ""; // Basic | Pro | Enterprise etc.

    public int MaxUsers { get; set; }
    public int ApiCallsPerMinute { get; set; }
    public int WebhooksCount { get; set; }
}
