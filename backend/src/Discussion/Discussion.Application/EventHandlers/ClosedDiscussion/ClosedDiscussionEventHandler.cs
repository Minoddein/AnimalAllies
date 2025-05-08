using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace Discussion.Application.EventHandlers.ClosedDiscussion;

public class ClosedDiscussionEventHandler: INotificationHandler<ClosedDiscussionDomainEvent>
{
    private readonly ILogger<ClosedDiscussionEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public ClosedDiscussionEventHandler(
        ILogger<ClosedDiscussionEventHandler> logger,
        IOutboxRepository outboxRepository, 
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ClosedDiscussionDomainEvent notification, CancellationToken cancellationToken)
    {
        var invalidationIntegrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);
        
        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Closed discussion with relation id {relationId}",
            notification.RelationId);
    }
}