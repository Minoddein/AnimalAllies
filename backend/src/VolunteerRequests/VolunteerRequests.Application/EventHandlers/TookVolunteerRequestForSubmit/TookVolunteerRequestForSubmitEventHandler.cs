using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.TookVolunteerRequestForSubmit;

public class TookVolunteerRequestForSubmitEventHandler: INotificationHandler<TookRequestForSubmitDomainEvent>
{
    private readonly ILogger<TookVolunteerRequestForSubmitEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public TookVolunteerRequestForSubmitEventHandler(
        ILogger<TookVolunteerRequestForSubmitEventHandler> logger, 
        IOutboxRepository outboxRepository, 
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        TookRequestForSubmitDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var notificationEvent = new SendNotificationCreateVolunteerRequestEvent(notification.UserId, notification.Email);
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(
            null, 
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + TagsConstants.VolunteerRequests.IN_WAITING),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + 
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
        ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Volunteer request from user {userId} took for submit", notification.UserId);
    }
}