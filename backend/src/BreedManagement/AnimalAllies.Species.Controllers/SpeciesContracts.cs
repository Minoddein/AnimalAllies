using AnimalAllies.Core.DTOs;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetBreedsBySpeciesId;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;
using AnimalAllies.Species.Contracts;

namespace AnimalAllies.Species.Presentation;

public class SpeciesContracts(
    GetSpeciesWithPaginationHandlerDapper getSpeciesWithPaginationHandlerDapper,
    GetBreedsBySpeciesIdWithPaginationHandlerDapper getBreedsBySpeciesIdWithPaginationHandlerDapper) : ISpeciesContracts
{
    private readonly GetBreedsBySpeciesIdWithPaginationHandlerDapper _getBreedsBySpeciesIdWithPaginationHandlerDapper =
        getBreedsBySpeciesIdWithPaginationHandlerDapper;

    private readonly GetSpeciesWithPaginationHandlerDapper _getSpeciesWithPaginationHandlerDapper =
        getSpeciesWithPaginationHandlerDapper;

    public async Task<List<Guid>> GetSpecies(CancellationToken cancellationToken = default)
    {
        Result<List<SpeciesDto>> species =
            await _getSpeciesWithPaginationHandlerDapper.Handle(cancellationToken).ConfigureAwait(false);
        if (species.IsFailure)
        {
            throw new ArgumentNullException("Species not found");
        }

        return [.. species.Value.Select(s => s.Id)];
    }

    public async Task<List<Guid>> GetBreedsBySpeciesId(
        Guid speciesId,
        CancellationToken cancellationToken = default)
    {
        Result<List<BreedDto>> breeds =
            await _getBreedsBySpeciesIdWithPaginationHandlerDapper.Handle(speciesId, cancellationToken)
                .ConfigureAwait(false);
        if (breeds.IsFailure)
        {
            throw new ArgumentNullException("Species not found");
        }

        return [.. breeds.Value.Select(b => b.Id)];
    }
}