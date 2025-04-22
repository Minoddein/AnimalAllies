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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteerById;

public class GetVolunteerByIdHandler : IQueryHandler<VolunteerDto, GetVolunteerByIdQuery>
{
    private const string REDIS_KEY = "volunteers_";
    
    private readonly HybridCache _hybridCache;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetVolunteerByIdHandler> _logger;
    private readonly IValidator<GetVolunteerByIdQuery> _validator;

    public GetVolunteerByIdHandler(
        ILogger<GetVolunteerByIdHandler> logger,
        [FromKeyedServices(Constraints.Context.PetManagement)]ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetVolunteerByIdQuery> validator, 
        HybridCache hybridCache)
    {
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
        _hybridCache = hybridCache;
    }
    
    
    public async Task<Result<VolunteerDto>> Handle(
        GetVolunteerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(15)
        };

        var volunteer = await _hybridCache.GetOrCreateAsync(
            key: REDIS_KEY + query.VolunteerId,
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
        
                parameters.Add("@VolunteerId", query.VolunteerId);

                var sql = new StringBuilder("""
                                            select 
                                            id,
                                            first_name,
                                            second_name,
                                            patronymic,
                                            description,
                                            email,
                                            phone_number,
                                            work_experience,
                                            requisites
                                            from volunteers.volunteers
                                            where id = @VolunteerId
                                            limit 1
                                            """);
                
                return await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
                    sql.ToString(),
                    (volunteer, requisites) =>
                    {
                        volunteer.Requisites = requisites;

                        return volunteer;
                    },
                    splitOn: "requisites",
                    param: parameters);
            },
            options: options,
            cancellationToken: cancellationToken);
        
        var result = volunteer.FirstOrDefault();

        if (result is null) 
            return Errors.General.NotFound();
        
        _logger.LogInformation("Get volunteer with id {VolunteerId}", query.VolunteerId);
        
        return result;
    }
}