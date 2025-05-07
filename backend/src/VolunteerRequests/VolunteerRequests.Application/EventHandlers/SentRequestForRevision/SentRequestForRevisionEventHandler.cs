using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.SentRequestForRevision;

public class SentRequestForRevisionEventHandler: INotificationHandler<SentRequestForRevisionDomainEvent>
{
    private readonly ILogger<SentRequestForRevisionEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public SentRequestForRevisionEventHandler(
        ILogger<SentRequestForRevisionEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        SentRequestForRevisionDomainEvent notification, 
        CancellationToken cancellationToken)
    {
        //TODO: Добавить уведомление, что заявка отправлена на доработку
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(
            null, 
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + 
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Volunteer request from user {userId} sent for revision", notification.UserId);
    }
}