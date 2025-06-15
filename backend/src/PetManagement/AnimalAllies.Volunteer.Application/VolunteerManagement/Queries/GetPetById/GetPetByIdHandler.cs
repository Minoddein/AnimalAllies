using System.Text;
using System.Text.Json;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetById;

public class GetPetByIdHandler : IQueryHandler<PetDto, GetPetByIdQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetPetByIdHandler> _logger;
    private readonly IValidator<GetPetByIdQuery> _validator;
    private readonly HybridCache _hybridCache;

    public GetPetByIdHandler(
        [FromKeyedServices(Constraints.Context.PetManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetPetByIdHandler> logger,
        IValidator<GetPetByIdQuery> validator,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
        _hybridCache = hybridCache;
    }

    public async Task<Result<PetDto>> Handle(GetPetByIdQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        parameters.Add("@PetId", query.PetId);

        var sql = new StringBuilder("""
                                    select 
                                    p.id as pet_id,
                                    volunteer_id,
                                    p.name as pet_name,
                                    city,
                                    state,
                                    street,
                                    zip_code,
                                    p.breed_id as pet_breed_id,
                                    p.species_id as pet_species_id,
                                    s.name as species_name,
                                    b.name as breed_name,
                                    help_status,
                                    phone_number,
                                    birth_date,
                                    color,
                                    height,
                                    weight,
                                    is_spayed_neutered,
                                    is_vaccinated,
                                    arrive_date,
                                    last_owner,
                                    "from",
                                    animal_sex as sex,
                                    last_vaccination_date,
                                    has_chronic_diseases,
                                    medical_notes,
                                    requires_special_diet,
                                    has_allergies,
                                    aggression_level,
                                    friendliness,
                                    activity_level,
                                    good_with_kids,
                                    good_with_people,
                                    good_with_other_animals,
                                    pet_details_description as description,
                                    health_information,
                                    position,
                                    requisites,
                                    pet_photos
                                    from volunteers.pets p
                                    inner join species.species s on p.species_id = s.id
                                    inner join species.breeds b on p.breed_id = b.id
                                        where p.id = @PetId limit 1
                                    """);
        
        var petsQuery = await connection.QueryAsync<PetDto, RequisiteDto[], PetPhotoDto[], PetDto>(
            sql.ToString(),
            (pet, requisites, petPhotoDtos) =>
            {
                pet.Requisites = requisites;

                pet.PetPhotos = petPhotoDtos;

                return pet;
            },
            splitOn: "requisites, pet_photos",
            param: parameters);

        var result = petsQuery.FirstOrDefault();

        if (result is null)
            return Errors.General.NotFound();

        _logger.LogInformation("Get pet with id {petId}", query.PetId);

        return result;
    }
}