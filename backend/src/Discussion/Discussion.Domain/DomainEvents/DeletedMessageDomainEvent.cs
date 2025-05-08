using AnimalAllies.SharedKernel.Shared;

namespace Discussion.Domain.DomainEvents;

public record DeletedMessageDomainEvent(Guid RelationId) : IDomainEvent;