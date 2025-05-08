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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CheckIfPetByBreedIdExist;

public class CheckIfPetByBreedIdExistHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<CheckIfPetByBreedIdExistHandler> logger) : IQueryHandler<bool, CheckIfPetByBreedIdExistQuery>
{
    private readonly ILogger<CheckIfPetByBreedIdExistHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<Result<bool>> Handle(
        CheckIfPetByBreedIdExistQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@BreedId", query.Id);
        StringBuilder sql = new("""
                                select 
                                    id
                                    from volunteers.pets
                                    where breed_id = @BreedId and
                                          is_deleted = false
                                    limit 1
                                """);

        List<PetDto> pets =
        [
            .. await connection.QueryAsync<PetDto>(
                sql.ToString(),
                parameters).ConfigureAwait(false)
        ];

        _logger.LogInformation("Get pets with breed id {breedId}", query.Id);

        if (pets.Count != 0)
        {
            return true;
        }

        return false;
    }
}