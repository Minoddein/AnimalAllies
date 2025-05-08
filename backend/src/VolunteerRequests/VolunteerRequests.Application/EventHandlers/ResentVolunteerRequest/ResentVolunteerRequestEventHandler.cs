using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using MediatR;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.ResentVolunteerRequest;

public class ResentVolunteerRequestEventHandler(
    ILogger<ResentVolunteerRequestEventHandler> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<ResentVolunteerRequestDomainEvent>
{
    private readonly ILogger<ResentVolunteerRequestEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(
        ResentVolunteerRequestDomainEvent notification,
        CancellationToken cancellationToken)
    {
        CacheInvalidateIntegrationEvent integrationEvent = new(
            null,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Volunteer request from user {userId} resent", notification.UserId);
    }
}