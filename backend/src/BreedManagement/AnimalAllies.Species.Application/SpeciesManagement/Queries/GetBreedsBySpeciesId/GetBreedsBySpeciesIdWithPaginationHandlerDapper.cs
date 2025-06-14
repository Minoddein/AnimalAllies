using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetBreedsBySpeciesId;

public class GetBreedsBySpeciesIdWithPaginationHandlerDapper: IQueryHandler<PagedList<BreedDto>, GetBreedsBySpeciesIdWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetBreedsBySpeciesIdWithPaginationQuery> _validator;
    private readonly ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> _logger;
    private readonly HybridCache _hybridCache;

    public GetBreedsBySpeciesIdWithPaginationHandlerDapper(
        [FromKeyedServices(Constraints.Context.BreedManagement)]ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> logger,
        IValidator<GetBreedsBySpeciesIdWithPaginationQuery> validator,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
        _hybridCache = hybridCache;
    }

    public async Task<Result<PagedList<BreedDto>>> Handle(GetBreedsBySpeciesIdWithPaginationQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();
        
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(3),
            LocalCacheExpiration = TimeSpan.FromMinutes(60)
        };
        
        var cachedBreeds = await _hybridCache.GetOrCreateAsync(
            key: $"{TagsConstants.BREEDS}_{query.SpeciesId}_{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
        
                parameters.Add("@SpeciesId", query.SpeciesId);

                var sql = new StringBuilder("""
                                            select 
                                                id,
                                                name,
                                                species_id
                                                from species.breeds
                                            where species_id = @SpeciesId
                                            """);

                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page,query.PageSize);
                
                return await connection.QueryAsync<BreedDto>(sql.ToString(), parameters);
            },
            options: options,
            tags: [new string(TagsConstants.BREEDS + "_" + query.SpeciesId)],
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Get breeds with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var breedsDtos = cachedBreeds.ToList();
        
        return new PagedList<BreedDto>
        {
            Items = breedsDtos.ToList(),
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = breedsDtos.Count
        };

    }
    
    public async Task<Result<List<BreedDto>>> Handle(Guid speciesId, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        
        parameters.Add("@SpeciesId", speciesId);

        var sql = new StringBuilder("""
                                    select 
                                        id as breed_id,
                                        name as breed_name,
                                        species_id
                                        from species.breeds
                                    where species_id = @SpeciesId
                                    """);
        
        var breeds = await connection.QueryAsync<BreedDto>(sql.ToString(), parameters);
        
        _logger.LogInformation("Get breeds with pagination with {speciesId}", speciesId);

        return breeds.ToList();
    }
    
}