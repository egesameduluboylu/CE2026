using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public sealed class Tenant : BaseEntity
    {
        public string Name { get; set; } = "";
    }
}
