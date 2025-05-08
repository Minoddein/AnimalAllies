using AnimalAllies.SharedKernel.Shared;

namespace Discussion.Domain.DomainEvents;

public record CreatedDiscussionDomainEvent(Guid RelationId) : IDomainEvent;