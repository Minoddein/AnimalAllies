using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class SpeciesCreatedEventHandler(
    ILogger<SpeciesCreatedEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<SpeciesCreatedDomainEvent>
{
    private readonly ILogger<SpeciesCreatedEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(SpeciesCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent integrationEvent = new(TagsConstants.SPECIES, null);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Species with id {id} created", notification.SpecieId);
    }
}