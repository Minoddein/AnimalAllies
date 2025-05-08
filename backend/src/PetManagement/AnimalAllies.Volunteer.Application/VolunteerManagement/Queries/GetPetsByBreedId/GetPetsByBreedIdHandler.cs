using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsByBreedId;

public class GetPetsByBreedIdHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetPetsByBreedIdHandler> logger,
    IValidator<GetPetsByBreedIdQuery> validator) : IQueryHandler<List<PetDto>, GetPetsByBreedIdQuery>
{
    private readonly ILogger<GetPetsByBreedIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetPetsByBreedIdQuery> _validator = validator;

    public async Task<Result<List<PetDto>>> Handle(
        GetPetsByBreedIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@BreedId", query.BreedId);

        StringBuilder sql = new("""
                                select 
                                    id,
                                    volunteer_id,
                                    name,
                                    city,
                                    state,
                                    street,
                                    zip_code,
                                    breed_id,
                                    species_id,
                                    help_status,
                                    phone_number,
                                    birth_date,
                                    color,
                                    height,
                                    weight,
                                    is_castrated,
                                    is_vaccinated,
                                    position,
                                    health_information,
                                    pet_details_description,
                                    requisites,
                                    pet_photos
                                    from volunteers.pets
                                    where breed_id = @BreedId and
                                          is_deleted = false
                                """);

        sql.ApplyPagination(query.Page, query.PageSize);

        IEnumerable<PetDto> petsQuery = await connection.QueryAsync<PetDto, RequisiteDto[], PetPhotoDto[], PetDto>(
            sql.ToString(),
            (pet, requisites, petPhotoDtos) =>
            {
                pet.Requisites = requisites;

                pet.PetPhotos = petPhotoDtos;

                return pet;
            },
            splitOn: "requisites, pet_photos",
            param: parameters).ConfigureAwait(false);

        _logger.LogInformation("Get pets with breed id {breedId}", query.BreedId);

        return petsQuery.ToList();
    }
}