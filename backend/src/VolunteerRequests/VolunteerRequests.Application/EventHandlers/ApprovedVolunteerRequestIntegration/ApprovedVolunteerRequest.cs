using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Contracts.Messaging;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.ApprovedVolunteerRequestIntegration;

public class ApprovedVolunteerRequest(
    ILogger<ApprovedVolunteerRequest> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<ApprovedVolunteerRequestDomainEvent>
{
    private readonly ILogger<ApprovedVolunteerRequest> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(ApprovedVolunteerRequestDomainEvent notification, CancellationToken cancellationToken)
    {
        ApprovedVolunteerRequestEvent approvedIntegrationEvent = new(
            notification.UserId,
            notification.FirstName,
            notification.SecondName,
            notification.Patronymic,
            notification.WorkExperience);

        await _outboxRepository.AddAsync(approvedIntegrationEvent, cancellationToken).ConfigureAwait(false);

        SendNotificationApproveVolunteerRequestEvent notificationIntegrationEvent = new(
            notification.UserId,
            notification.Email);

        CacheInvalidateIntegrationEvent invalidationIntegrationEvent = new(
            null,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(approvedIntegrationEvent, cancellationToken).ConfigureAwait(false);

        await _outboxRepository.AddAsync(notificationIntegrationEvent, cancellationToken).ConfigureAwait(false);

        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Sent integration event for creation volunteer account for user with id {userId}",
            notification.UserId);
    }
}