using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public sealed class ApiKey : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } = "";

        // first characters of the raw secret, to identify keys without storing the secret
        [MaxLength(16)]
        public string Prefix { get; set; } = "";

        [MaxLength(128)]
        public string SecretHash { get; set; } = "";

        // comma-separated for MVP
        [MaxLength(500)]
        public string Scopes { get; set; } = "";

        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public DateTimeOffset? LastUsedAt { get; set; }
    }
}
