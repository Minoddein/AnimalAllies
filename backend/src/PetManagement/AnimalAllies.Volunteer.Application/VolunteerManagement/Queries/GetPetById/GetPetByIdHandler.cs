using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetById;

public class GetPetByIdHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetPetByIdHandler> logger,
    IValidator<GetPetByIdQuery> validator,
    HybridCache hybridCache) : IQueryHandler<PetDto, GetPetByIdQuery>
{
    private readonly ILogger<GetPetByIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetPetByIdQuery> _validator = validator;

    public async Task<Result<PetDto>> Handle(GetPetByIdQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@PetId", query.PetId);

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
                                    where id = @PetId limit 1
                                """);

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

        PetDto? result = petsQuery.FirstOrDefault();

        if (result is null)
        {
            return Errors.General.NotFound();
        }

        _logger.LogInformation("Get pet with id {petId}", query.PetId);

        return result;
    }
}