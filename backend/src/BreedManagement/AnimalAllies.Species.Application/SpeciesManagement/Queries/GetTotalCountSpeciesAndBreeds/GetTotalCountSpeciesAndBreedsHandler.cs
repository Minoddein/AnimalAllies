using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Species.Contracts;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetTotalCountSpeciesAndBreeds;

public class GetTotalCountSpeciesAndBreedsHandler :
    IQueryHandler<TotalCountSpeciesAndBreedsResponse, GetTotalCountSpeciesAndBreedsQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetTotalCountSpeciesAndBreedsHandler> _logger;

    public GetTotalCountSpeciesAndBreedsHandler(
        [FromKeyedServices(Constraints.Context.BreedManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetTotalCountSpeciesAndBreedsHandler> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<TotalCountSpeciesAndBreedsResponse>> Handle(
        GetTotalCountSpeciesAndBreedsQuery query, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();

        var totalSpeciesCount = await connection
            .ExecuteScalarAsync<int>(
                "select count(id) from species.species ");

        var totalBreedCount = await connection.ExecuteScalarAsync<int>("select count(id) from species.breeds ");

        var result = new TotalCountSpeciesAndBreedsResponse(totalSpeciesCount, totalBreedCount);
        
        _logger.LogInformation("Got species and breeds count");

        return result;
    }
}