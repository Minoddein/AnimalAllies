using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.EventHandlers;

public class UserAddedSocialNetworkEventHandler(
    ILogger<UserAddedAvatarEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<UserAddedSocialNetworkDomainEvent>
{
    private readonly ILogger<UserAddedAvatarEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(UserAddedSocialNetworkDomainEvent notification, CancellationToken cancellationToken)
    {
        string key = TagsConstants.USERS + "_" + notification.UserId;

        CacheInvalidateIntegrationEvent integrationEvent = new(key, null);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("User {NotificationUserId} has been added social networks", notification.UserId);
    }
}