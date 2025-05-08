using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.VolunteerRequestRejected;

public class VolunteerRequestRejectedEventHandler(
    ILogger<VolunteerRequestRejectedEventHandler> logger,
    IProhibitionSendingRepository repository,
    IDateTimeProvider dateTimeProvider,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWork) : INotificationHandler<VolunteerRequestRejectedDomainEvent>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<VolunteerRequestRejectedEventHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IProhibitionSendingRepository _repository = repository;
    private readonly IUnitOfWorkOutbox _unitOfWork = unitOfWork;

    public async Task Handle(VolunteerRequestRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        ProhibitionSendingId prohibitionSendingId = ProhibitionSendingId.NewGuid();
        Result<ProhibitionSending> prohibitionSending = ProhibitionSending.Create(
            prohibitionSendingId,
            notification.UserId,
            _dateTimeProvider.UtcNow);

        if (prohibitionSending.IsFailure)
        {
            throw new Exception(prohibitionSending.Errors.ToString());
        }

        Result<ProhibitionSendingId> prohibitionSendingResult =
            await _repository.Create(prohibitionSending.Value, cancellationToken).ConfigureAwait(false);

        if (prohibitionSendingResult.IsFailure)
        {
            throw new Exception(prohibitionSendingResult.Errors.ToString());
        }

        SendNotificationRejectVolunteerRequestEvent notificationEvent = new(
            notification.UserId,
            notification.Email,
            notification.RejectionComment);

        CacheInvalidateIntegrationEvent integrationEvent = new(
            null,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "User was prohibited for creating request with id {UserId}",
            notification.UserId);
    }
}