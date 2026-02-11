using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Infrastructure.Persistence.Entities;

public sealed class WebhookDelivery
{
    public Guid Id { get; set; }

    public Guid WebhookId { get; set; }

    [MaxLength(100)]
    public string EventType { get; set; } = "";

    [MaxLength(500)]
    public string PayloadUrl { get; set; } = "";

    public string Payload { get; set; } = "";

    public string Headers { get; set; } = "";

    public int AttemptCount { get; set; } = 0;

    public int MaxAttempts { get; set; } = 3;

    public DateTimeOffset? LastAttemptAt { get; set; }

    public string? LastResponse { get; set; }

    public int? LastStatusCode { get; set; }

    public bool IsDelivered { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Webhook Webhook { get; set; } = null!;
}
