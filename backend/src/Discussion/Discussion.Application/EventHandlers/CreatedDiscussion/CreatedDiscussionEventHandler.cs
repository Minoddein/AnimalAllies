using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace Discussion.Application.EventHandlers.CreatedDiscussion;

public class CreatedDiscussionEventHandler(
    ILogger<CreatedDiscussionEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<CreatedDiscussionDomainEvent>
{
    private readonly ILogger<CreatedDiscussionEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(CreatedDiscussionDomainEvent notification, CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent invalidationIntegrationEvent = new(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);

        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Created discussion with relation id {relationId}",
            notification.RelationId);
    }
}