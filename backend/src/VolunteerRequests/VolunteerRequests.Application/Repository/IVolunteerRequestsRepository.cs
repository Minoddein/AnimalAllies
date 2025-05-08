using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Repository;

public interface IVolunteerRequestsRepository
{
    Task<Result<VolunteerRequestId>> Create(VolunteerRequest entity, CancellationToken cancellationToken = default);

    Task<Result<VolunteerRequest>> GetById(VolunteerRequestId id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<VolunteerRequest>>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default);

    Result<VolunteerRequestId> Delete(VolunteerRequest entity);
}