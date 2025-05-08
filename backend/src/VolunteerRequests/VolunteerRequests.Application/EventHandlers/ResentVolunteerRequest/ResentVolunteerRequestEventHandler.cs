using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.ResentVolunteerRequest;

public class ResentVolunteerRequestEventHandler: INotificationHandler<ResentVolunteerRequestDomainEvent>
{
    private readonly ILogger<ResentVolunteerRequestEventHandler> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public ResentVolunteerRequestEventHandler(
        ILogger<ResentVolunteerRequestEventHandler> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ResentVolunteerRequestDomainEvent notification,
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

        _logger.LogInformation("Volunteer request from user {userId} resent", notification.UserId);
    }
}