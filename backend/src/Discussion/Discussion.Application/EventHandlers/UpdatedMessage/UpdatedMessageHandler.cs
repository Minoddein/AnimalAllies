using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using Discussion.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace Discussion.Application.EventHandlers.UpdatedMessage;

public class UpdatedMessageHandler: INotificationHandler<UpdatedMessageDomainEvent>
{
    private readonly ILogger<UpdatedMessageHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public UpdatedMessageHandler(
        ILogger<UpdatedMessageHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        var invalidationIntegrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [
                new string(TagsConstants.DISCUSSIONS + "_" + notification.RelationId)
            ]);
        
        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Updated message from discussion with relation id {relationId}",
            notification.RelationId);
    }
}