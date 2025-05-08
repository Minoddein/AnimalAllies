using MediatR;

namespace AnimalAllies.Accounts.Domain.DomainEvents;

public record UserAddedSocialNetworkDomainEvent(Guid UserId) : INotification;