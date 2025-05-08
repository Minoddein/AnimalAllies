using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Repository;

public interface IProhibitionSendingRepository
{
    Task<Result<ProhibitionSendingId>> Create(ProhibitionSending entity, CancellationToken cancellationToken = default);

    Task<Result<ProhibitionSending?>> GetById(ProhibitionSendingId id, CancellationToken cancellationToken = default);

    Task<Result<ProhibitionSending>> GetByUserId(Guid userId, CancellationToken cancellationToken = default);

    Result Delete(ProhibitionSending entity);
}