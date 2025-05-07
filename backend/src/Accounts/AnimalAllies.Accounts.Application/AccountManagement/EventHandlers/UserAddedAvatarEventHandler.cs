using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.EventHandlers;

public class UserAddedAvatarEventHandler: INotificationHandler<UserAddedAvatarDomainEvent>
{
    private readonly ILogger<UserAddedAvatarEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public UserAddedAvatarEventHandler(
        ILogger<UserAddedAvatarEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserAddedAvatarDomainEvent notification, CancellationToken cancellationToken)
    {
        var key = TagsConstants.USERS + "_" + notification.UserId;
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(key, null);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("User with id {id} added avatar", notification.UserId);
    }
}