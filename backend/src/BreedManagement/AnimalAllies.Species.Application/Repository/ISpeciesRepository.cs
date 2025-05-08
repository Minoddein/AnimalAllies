using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;

namespace AnimalAllies.Species.Application.Repository;

public interface ISpeciesRepository
{
    Task<Result<SpeciesId>> Create(Domain.Species entity, CancellationToken cancellationToken = default);

    Result<SpeciesId> Delete(Domain.Species entity, CancellationToken cancellationToken = default);

    Result<SpeciesId> Save(Domain.Species entity, CancellationToken cancellationToken = default);

    Task<Result<Domain.Species>> GetById(SpeciesId id, CancellationToken cancellationToken = default);

    Task<Result<List<Domain.Species>>> Get(CancellationToken cancellationToken = default);
}