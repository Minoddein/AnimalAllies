using AnimalAllies.SharedKernel.Exceptions;
using AnimalAllies.SharedKernel.Shared;
using MediatR;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.EventHandlers.ProhibitionOnVolunteerRequestChecked;

public class ProhibitionOnVolunteerRequestChecked(
    IProhibitionSendingRepository prohibitionSendingRepository)
    : INotificationHandler<ProhibitionOnVolunteerRequestCheckedEvent>
{
    private const int REQUEST_BLOCKING_PERIOD = 7;

    private readonly IProhibitionSendingRepository _prohibitionSendingRepository = prohibitionSendingRepository;

    public async Task Handle(
        ProhibitionOnVolunteerRequestCheckedEvent notification,
        CancellationToken cancellationToken)
    {
        Result<ProhibitionSending> prohibitionSending = await _prohibitionSendingRepository
            .GetByUserId(notification.UserId, cancellationToken).ConfigureAwait(false);

        if (prohibitionSending.IsSuccess)
        {
            Result checkResult = prohibitionSending.Value.CheckExpirationOfProhibtion(REQUEST_BLOCKING_PERIOD);

            if (checkResult.IsFailure)
            {
                throw new AccountBannedException(checkResult.Errors.ToString());
            }

            Result result = _prohibitionSendingRepository.Delete(prohibitionSending.Value);
            if (result.IsFailure)
            {
                throw new AccountBannedException(checkResult.Errors.ToString());
            }
        }
    }
}