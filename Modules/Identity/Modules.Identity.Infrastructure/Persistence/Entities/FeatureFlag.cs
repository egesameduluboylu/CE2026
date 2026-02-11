using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public sealed class FeatureFlag : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string? Description { get; set; }
    }
}
