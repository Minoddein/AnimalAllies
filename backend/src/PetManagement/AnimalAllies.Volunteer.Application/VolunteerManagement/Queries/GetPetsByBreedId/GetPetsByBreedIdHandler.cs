using System.Text;
using System.Text.Json;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsByBreedId;

public class GetPetsByBreedIdHandler: IQueryHandler<List<PetDto>, GetPetsByBreedIdQuery>
{
    private const string REDIS_KEY = "pets_";
    
    private readonly HybridCache _hybridCache;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetPetsByBreedIdHandler> _logger;
    private readonly IValidator<GetPetsByBreedIdQuery> _validator;

    public GetPetsByBreedIdHandler(
        [FromKeyedServices(Constraints.Context.PetManagement)]ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetPetsByBreedIdHandler> logger,
        IValidator<GetPetsByBreedIdQuery> validator,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
        _hybridCache = hybridCache;
    }

    public async Task<Result<List<PetDto>>> Handle(
        GetPetsByBreedIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();
        
        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        
        parameters.Add("@BreedId", query.BreedId);
        
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
                                        where breed_id = @BreedId and
                                              is_deleted = false
                                    """);
        
        sql.ApplyPagination(query.Page, query.PageSize);
        
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(8)
        };
        
        var cachedPets = await _hybridCache.GetOrCreateAsync(
            key:  $"{REDIS_KEY}{query.BreedId}_{query.Page}_{query.PageSize}",
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
        
        _logger.LogInformation("Get pets with breed id {breedId}", query.BreedId);

        return cachedPets.ToList();
    }
}