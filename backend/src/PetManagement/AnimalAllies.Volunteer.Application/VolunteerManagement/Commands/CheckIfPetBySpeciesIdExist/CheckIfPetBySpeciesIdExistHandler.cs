using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CheckIfPetBySpeciesIdExist;

public class CheckIfPetBySpeciesIdExistHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<CheckIfPetBySpeciesIdExistHandler> logger) : IQueryHandler<bool, CheckIfPetBySpeciesIdExistQuery>
{
    private readonly ILogger<CheckIfPetBySpeciesIdExistHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<Result<bool>> Handle(
        CheckIfPetBySpeciesIdExistQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@SpeciesId", query.Id);
        StringBuilder sql = new("""
                                select 
                                    id
                                    from volunteers.pets
                                    where species_id = @SpeciesId and
                                          is_deleted = false
                                    limit 1
                                """);

        List<PetDto> pets =
        [
            .. await connection.QueryAsync<PetDto>(
                sql.ToString(),
                parameters).ConfigureAwait(false)
        ];

        _logger.LogInformation("Get pets with species id {speciesId}", query.Id);

        if (pets.Count != 0)
        {
            return true;
        }

        return false;
    }
}