using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.EventHandlers;

public class UserInfoUpdatedEventHandler: INotificationHandler<UserInfoUpdatedDomainEvent>
{
    private readonly ILogger<UserInfoUpdatedEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public UserInfoUpdatedEventHandler(
        ILogger<UserInfoUpdatedEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserInfoUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var key = TagsConstants.USERS + "_" + notification.UserId;
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(key, null);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("User {NotificationUserId} has been updated", notification.UserId);
    }
}