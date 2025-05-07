using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.CreatedVolunteerRequest;

public class CreatedVolunteerRequestEventHandler: INotificationHandler<CreatedVolunteerRequestDomainEvent>
{
    private readonly ILogger<CreatedVolunteerRequestEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public CreatedVolunteerRequestEventHandler(
        ILogger<CreatedVolunteerRequestEventHandler> logger, 
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreatedVolunteerRequestDomainEvent notification, CancellationToken cancellationToken)
    {
        var notificationEvent = new SendNotificationCreateVolunteerRequestEvent(notification.UserId, notification.Email);
        
        var tag = new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + TagsConstants.VolunteerRequests.IN_WAITING);
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(null, [tag]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("User with id {id} created volunteer request", notification.UserId);
    }
}