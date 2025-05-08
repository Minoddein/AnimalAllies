using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Accounts.Domain.DomainEvents;

public record UserAddedAvatarDomainEvent(Guid UserId) : IDomainEvent;