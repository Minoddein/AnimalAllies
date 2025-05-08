using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace Discussion.Application.EventHandlers.PostedMessage;

public class PostedMessageEventHandler(
    ILogger<PostedMessageEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<PostedMessageDomainEvent>
{
    private readonly ILogger<PostedMessageEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(PostedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent invalidationIntegrationEvent = new(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);

        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Posted message to discussion with relation id {relationId}",
            notification.RelationId);
    }
}