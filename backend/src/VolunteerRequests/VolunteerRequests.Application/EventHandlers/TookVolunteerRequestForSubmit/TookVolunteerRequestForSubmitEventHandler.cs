using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.TookVolunteerRequestForSubmit;

public class TookVolunteerRequestForSubmitEventHandler(
    ILogger<TookVolunteerRequestForSubmitEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<TookRequestForSubmitDomainEvent>
{
    private readonly ILogger<TookVolunteerRequestForSubmitEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(
        TookRequestForSubmitDomainEvent notification,
        CancellationToken cancellationToken)
    {
        SendNotificationCreateVolunteerRequestEvent notificationEvent = new(notification.UserId, notification.Email);

        CacheInvalidateIntegrationEvent integrationEvent = new(
            null,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + TagsConstants.VolunteerRequests.IN_WAITING),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Volunteer request from user {userId} took for submit", notification.UserId);
    }
}