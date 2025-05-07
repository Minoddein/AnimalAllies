using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.EventHandlers;

public class UserAddedSocialNetworkEventHandler: INotificationHandler<UserAddedSocialNetworkDomainEvent>
{
    private readonly ILogger<UserAddedAvatarEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public UserAddedSocialNetworkEventHandler(
        ILogger<UserAddedAvatarEventHandler> logger,
        IOutboxRepository outboxRepository, 
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserAddedSocialNetworkDomainEvent notification, CancellationToken cancellationToken)
    {
        var key = TagsConstants.USERS + "_" + notification.UserId;
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(key);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("User {NotificationUserId} has been added social networks", notification.UserId);
    }
}