using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.UpdatedVolunteerRequest;

public class UpdatedVolunteerRequestEventHandler: INotificationHandler<UpdatedVolunteerRequestDomainEvent>
{
    private readonly ILogger<UpdatedVolunteerRequestEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public UpdatedVolunteerRequestEventHandler(
        ILogger<UpdatedVolunteerRequestEventHandler> logger, 
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdatedVolunteerRequestDomainEvent notification, 
        CancellationToken cancellationToken)
    {
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

        _logger.LogInformation("Volunteer request from user {userId} updated", notification.UserId);
    }
}