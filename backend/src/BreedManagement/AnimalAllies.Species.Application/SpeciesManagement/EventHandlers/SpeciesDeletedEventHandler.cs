using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class SpeciesDeletedEventHandler(
    ILogger<SpeciesCreatedEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<SpeciesDeletedDomainEvent>
{
    private readonly ILogger<SpeciesCreatedEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(SpeciesDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent integrationEvent = new(
            null,
            [
                new string(TagsConstants.BREEDS + "_" + notification.SpeciesId),
                TagsConstants.SPECIES
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Species with id {id} deleted", notification.SpeciesId);
    }
}