using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.CreatedVolunteerRequest;

public class CreatedVolunteerRequestEventHandler(
    ILogger<CreatedVolunteerRequestEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<CreatedVolunteerRequestDomainEvent>
{
    private readonly ILogger<CreatedVolunteerRequestEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(CreatedVolunteerRequestDomainEvent notification, CancellationToken cancellationToken)
    {
        SendNotificationCreateVolunteerRequestEvent notificationEvent = new(notification.UserId, notification.Email);

        string tag = new(TagsConstants.VOLUNTEER_REQUESTS + "_" + TagsConstants.VolunteerRequests.IN_WAITING);

        CacheInvalidateIntegrationEvent integrationEvent = new(null, [tag]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("User with id {id} created volunteer request", notification.UserId);
    }
}