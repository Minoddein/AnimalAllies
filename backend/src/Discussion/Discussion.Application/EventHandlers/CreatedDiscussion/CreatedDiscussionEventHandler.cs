using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;
using VolunteerRequests.Contracts.Messaging;

namespace Discussion.Application.EventHandlers.CreatedDiscussion;

public class CreatedDiscussionEventHandler: INotificationHandler<CreatedDiscussionDomainEvent>
{
    private readonly ILogger<CreatedDiscussionEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public CreatedDiscussionEventHandler(
        ILogger<CreatedDiscussionEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreatedDiscussionDomainEvent notification, CancellationToken cancellationToken)
    {
        var invalidationIntegrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);
        
        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Created discussion with relation id {relationId}",
            notification.RelationId);
    }
}