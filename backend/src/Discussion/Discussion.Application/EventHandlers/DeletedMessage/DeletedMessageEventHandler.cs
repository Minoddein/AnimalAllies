using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace Discussion.Application.EventHandlers.DeletedMessage;

public class DeletedMessageEventHandler: INotificationHandler<DeletedMessageDomainEvent>
{
    private readonly ILogger<DeletedMessageEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public DeletedMessageEventHandler(
        ILogger<DeletedMessageEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        var invalidationIntegrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);
        
        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Deleted message from discussion with relation id {relationId}",
            notification.RelationId);
    }
}