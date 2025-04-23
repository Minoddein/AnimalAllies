using AnimalAllies.Species.Contracts.Events;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class BreedCreatedEventHandler: INotificationHandler<BreedCreatedDomainEvent>
{
    private readonly ILogger<BreedCreatedEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;
    private readonly IMemoryCache _memoryCache;

    public BreedCreatedEventHandler(
        ILogger<BreedCreatedEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _memoryCache = memoryCache;
    }

    public async Task Handle(BreedCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new BreedCreatedIntegrationEvent(notification.SpeciesId, notification.BreedId);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _memoryCache.Remove($"species_{notification.SpeciesId}");

        _logger.LogInformation("Breed with id {id} created", notification.BreedId);
    }
}