namespace Modules.Identity.Domain.Events;

public record UserRegisteredDomainEvent(Guid UserId, string Email, DateTimeOffset OccurredAt) : IDomainEvent;

public interface IDomainEvent;
