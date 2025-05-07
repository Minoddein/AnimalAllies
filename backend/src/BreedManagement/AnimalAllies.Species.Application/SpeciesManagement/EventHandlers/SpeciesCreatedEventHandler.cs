using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class SpeciesCreatedEventHandler: INotificationHandler<SpeciesCreatedDomainEvent>
{
    private readonly ILogger<SpeciesCreatedEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public SpeciesCreatedEventHandler(
        ILogger<SpeciesCreatedEventHandler> logger, 
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SpeciesCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new CacheInvalidateIntegrationEvent(TagsConstants.SPECIES, null);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("Species with id {id} created", notification.SpecieId);
    }
}