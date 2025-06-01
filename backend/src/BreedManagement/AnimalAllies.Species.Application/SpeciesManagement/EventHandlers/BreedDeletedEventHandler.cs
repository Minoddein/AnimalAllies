using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class BreedDeletedEventHandler: INotificationHandler<BreedDeletedDomainEvent>
{
    private readonly ILogger<BreedCreatedEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public BreedDeletedEventHandler(
        ILogger<BreedCreatedEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BreedDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [TagsConstants.SPECIES, TagsConstants.BREEDS + "_" + notification.SpeciesId]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("Breed with id {id} created", notification.BreedId);
    }
}