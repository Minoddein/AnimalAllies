using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain.DomainEvents;
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
    private readonly IMemoryCache _memoryCache;

    public UserAddedSocialNetworkEventHandler(
        ILogger<UserAddedAvatarEventHandler> logger,
        IOutboxRepository outboxRepository, 
        IUnitOfWorkOutbox unitOfWork,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _memoryCache = memoryCache;
    }

    public async Task Handle(UserAddedSocialNetworkDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserAddedSocialNetworkIntegrationEvent(notification.UserId);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _memoryCache.Remove($"users_{notification.UserId}");
        
        _logger.LogInformation("User {NotificationUserId} has been added social networks", notification.UserId);
    }
}