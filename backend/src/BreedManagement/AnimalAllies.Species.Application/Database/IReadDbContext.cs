using AnimalAllies.Core.DTOs;

namespace AnimalAllies.Species.Application.Database;

public interface IReadDbContext
{
    IQueryable<BreedDto> Breeds { get; }

    IQueryable<SpeciesDto> Species { get; }
}