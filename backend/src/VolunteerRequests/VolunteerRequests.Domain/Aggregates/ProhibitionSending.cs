using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace VolunteerRequests.Domain.Aggregates;

public class ProhibitionSending : Entity<ProhibitionSendingId>
{
    private ProhibitionSending(ProhibitionSendingId id)
        : base(id)
    {
    }

    private ProhibitionSending(ProhibitionSendingId id, Guid userId, DateTime bannedAt)
        : base(id)
    {
        UserId = userId;
        BannedAt = bannedAt;
    }

    public Guid UserId { get; private set; }

    public DateTime BannedAt { get; }

    public Result CheckExpirationOfProhibtion(int requestBlockingPeriod)
    {
        if (BannedAt.AddDays(requestBlockingPeriod) > DateTime.Now)
        {
            return Error.Failure(
                "account.banned",
                "The user cannot submit a request within a week");
        }

        return Result.Success();
    }

    public static Result<ProhibitionSending> Create(ProhibitionSendingId prohibitionSendingId, Guid userId,
        DateTime bannedAt)
    {
        if (prohibitionSendingId.Id == Guid.Empty)
        {
            return Errors.General.ValueIsRequired("banned user id");
        }

        if (userId == Guid.Empty)
        {
            return Errors.General.ValueIsRequired("user id");
        }

        if (bannedAt > DateTime.Now)
        {
            return Errors.General.ValueIsInvalid("banned at");
        }

        return new ProhibitionSending(prohibitionSendingId, userId, bannedAt);
    }
}