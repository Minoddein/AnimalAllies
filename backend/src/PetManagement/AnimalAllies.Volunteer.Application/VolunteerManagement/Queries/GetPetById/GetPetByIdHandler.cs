using System.Text;
using System.Text.Json;
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
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetById;

public class GetPetByIdHandler : IQueryHandler<PetDto, GetPetByIdQuery>
{
    private const string REDIS_KEY = "pets_";
    
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetPetByIdHandler> _logger;
    private readonly IValidator<GetPetByIdQuery> _validator;
    private readonly HybridCache _hybridCache;

    public GetPetByIdHandler(
        [FromKeyedServices(Constraints.Context.PetManagement)]ISqlConnectionFactory sqlConnectionFactory,
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
                                    """);
        
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(15)
        };
        
        var cachedPet = await _hybridCache.GetOrCreateAsync(
            key:  $"{REDIS_KEY}{query.PetId}",
            factory: async _ =>
            {
                return await connection.QueryAsync<PetDto, RequisiteDto[], PetPhotoDto[], PetDto>(
                    sql.ToString(),
                    (pet, requisites, petPhotoDtos) =>
                    {
                        pet.Requisites = requisites;
                    
                        pet.PetPhotos = petPhotoDtos;
                    
                        return pet;
                    },
                    splitOn:"requisites, pet_photos",
                    param: parameters);
            },
            options: options,
            cancellationToken: cancellationToken);
        
        var result = cachedPet.FirstOrDefault();

        if (result is null) 
            return Errors.General.NotFound();
        
        _logger.LogInformation("Get pet with id {petId}", query.PetId);
        
        return result;
    }
}