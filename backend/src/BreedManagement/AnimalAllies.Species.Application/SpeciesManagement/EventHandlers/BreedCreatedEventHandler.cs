using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class BreedCreatedEventHandler(
    ILogger<BreedCreatedEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<BreedCreatedDomainEvent>
{
    private readonly ILogger<BreedCreatedEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(BreedCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent integrationEvent = new(
            null,
            [new string(TagsConstants.BREEDS + "_" + notification.SpeciesId)]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Breed with id {id} created", notification.BreedId);
    }
}