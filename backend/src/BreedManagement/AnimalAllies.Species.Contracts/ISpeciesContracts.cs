namespace AnimalAllies.Species.Contracts;

public interface ISpeciesContracts
{
    Task<List<Guid>> GetSpecies(CancellationToken cancellationToken = default);

    Task<List<Guid>> GetBreedsBySpeciesId(Guid speciesId, CancellationToken cancellationToken = default);
}