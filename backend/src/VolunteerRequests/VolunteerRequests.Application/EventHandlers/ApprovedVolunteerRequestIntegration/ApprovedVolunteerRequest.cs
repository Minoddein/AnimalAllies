using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Contracts.Messaging;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.ApprovedVolunteerRequestIntegration;

public class ApprovedVolunteerRequest : INotificationHandler<ApprovedVolunteerRequestDomainEvent>
{
    private readonly ILogger<ApprovedVolunteerRequest> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public ApprovedVolunteerRequest(
        ILogger<ApprovedVolunteerRequest> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task Handle(ApprovedVolunteerRequestDomainEvent notification, CancellationToken cancellationToken)
    {
        var approvedIntegrationEvent = new ApprovedVolunteerRequestEvent(
            notification.UserId,
            notification.FirstName,
            notification.SecondName,
            notification.Patronymic,
            notification.WorkExperience,
            notification.Description,
            notification.Email,
            notification.Phone);

        await _outboxRepository.AddAsync(approvedIntegrationEvent, cancellationToken);

        var notificationIntegrationEvent = new SendNotificationApproveVolunteerRequestEvent(
            notification.UserId,
            notification.Email);

        var invalidationIntegrationEvent = new CacheInvalidateIntegrationEvent(
            null,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(approvedIntegrationEvent, cancellationToken);

        await _outboxRepository.AddAsync(notificationIntegrationEvent, cancellationToken);
        
        await _outboxRepository.AddAsync(invalidationIntegrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Sent integration event for creation volunteer account for user with id {userId}",
            notification.UserId);
    }
}