using AnimalAllies.SharedKernel.Shared;

namespace Discussion.Domain.DomainEvents;

public record UpdatedMessageDomainEvent(Guid RelationId) : IDomainEvent;