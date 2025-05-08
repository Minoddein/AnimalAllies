using AnimalAllies.SharedKernel.Shared;

namespace Discussion.Domain.DomainEvents;

public record PostedMessageDomainEvent(Guid RelationId) : IDomainEvent;