using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Accounts.Domain.DomainEvents;

public record UserInfoUpdatedDomainEvent(Guid UserId) : IDomainEvent;
