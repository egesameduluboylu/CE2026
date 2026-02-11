using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public sealed class Webhook : BaseEntity
    {
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string Url { get; set; } = "";

        // comma-separated list of event types (MVP)
        [MaxLength(500)]
        public string Events { get; set; } = "";

        // HMAC secret (SHA256 base64)
        [MaxLength(100)]
        public string Secret { get; set; } = "";

        public DateTimeOffset? DisabledAt { get; set; }
    }
}
