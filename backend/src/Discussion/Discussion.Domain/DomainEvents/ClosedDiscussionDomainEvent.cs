using AnimalAllies.SharedKernel.Shared;

namespace Discussion.Domain.DomainEvents;

public record ClosedDiscussionDomainEvent(Guid RelationId) : IDomainEvent;