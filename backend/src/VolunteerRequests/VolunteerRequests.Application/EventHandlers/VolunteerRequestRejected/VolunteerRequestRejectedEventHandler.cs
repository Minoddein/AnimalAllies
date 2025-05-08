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

public class VolunteerRequestRejectedEventHandler: INotificationHandler<VolunteerRequestRejectedDomainEvent>
{
    private readonly ILogger<VolunteerRequestRejectedEventHandler> _logger;
    private readonly IProhibitionSendingRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWork;

    public VolunteerRequestRejectedEventHandler(
        ILogger<VolunteerRequestRejectedEventHandler> logger,
        IProhibitionSendingRepository repository, 
        IDateTimeProvider dateTimeProvider, 
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWork)
    {
        _logger = logger;
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(VolunteerRequestRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        
        var prohibitionSendingId = ProhibitionSendingId.NewGuid();
        var prohibitionSending = ProhibitionSending.Create(
            prohibitionSendingId,
            notification.UserId,
            _dateTimeProvider.UtcNow);

        if (prohibitionSending.IsFailure)
            throw new Exception(prohibitionSending.Errors.ToString());
            
        var prohibitionSendingResult = await _repository.Create(prohibitionSending.Value, cancellationToken);

        if (prohibitionSendingResult.IsFailure)
            throw new Exception(prohibitionSendingResult.Errors.ToString());
        
        var notificationEvent = new SendNotificationRejectVolunteerRequestEvent(
            notification.UserId,
            notification.Email,
            notification.RejectionComment);
        
        var integrationEvent = new CacheInvalidateIntegrationEvent(
            null, 
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + notification.UserId),
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + 
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + notification.AdminId)
            ]);

        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);
        await _outboxRepository.AddAsync(notificationEvent, cancellationToken);

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("User was prohibited for creating request with id {UserId}",
            notification.UserId);
    }
}