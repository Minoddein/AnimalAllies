using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;

namespace Discussion.Application.Repository;

public interface IDiscussionRepository
{
    Task<Result<DiscussionId>> Create(
        Domain.Aggregate.Discussion entity,
        CancellationToken cancellationToken = default);

    Task<Result<Domain.Aggregate.Discussion>> GetById(DiscussionId id, CancellationToken cancellationToken = default);

    Task<Result<Domain.Aggregate.Discussion>> GetByRelationId(
        Guid relationId,
        CancellationToken cancellationToken = default);

    Result<DiscussionId> Delete(Domain.Aggregate.Discussion entity);
}