using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetAllSpeciesWithBreeds;

public class GetAllSpeciesWithBreedsHandler : IQueryHandler<List<SpeciesDto>, GetAllSpeciesWithBreedsQuery>
{
    private readonly ILogger<GetAllSpeciesWithBreedsHandler> _logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetAllSpeciesWithBreedsHandler(
        ILogger<GetAllSpeciesWithBreedsHandler> logger,
        [FromKeyedServices(Constraints.Context.BreedManagement)]
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<SpeciesDto>>> Handle(
        GetAllSpeciesWithBreedsQuery query, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();
        
        var speciesSql = "SELECT id as species_id, name as species_name FROM species.species";
        var speciesList = (await connection.QueryAsync<SpeciesDto>(speciesSql)).ToList();

        if (!speciesList.Any())
        {
            return new List<SpeciesDto>();
        }

        var speciesIds = speciesList.Select(s => s.SpeciesId).ToList();
        
        var breedsSql = """
                        SELECT 
                            b.id as breed_id,
                            b.name as breed_name,
                            b.species_id as species_id
                        FROM species.breeds b
                        WHERE b.species_id = ANY(@SpeciesIds)
                        """;

        var breeds = await connection.QueryAsync<BreedDto>(breedsSql,
            new { SpeciesIds = speciesIds });
        
        var breedsLookup = breeds.GroupBy(b => b.SpeciesId)
            .ToDictionary(g => g.Key, g => g.ToArray());
        
        foreach (var species in speciesList)
        {
            species.Breeds = breedsLookup.TryGetValue(species.SpeciesId, out var speciesBreeds) 
                ? speciesBreeds 
                : [];
        }

        _logger.LogInformation("Got species with breeds");
        return speciesList;
    }
}