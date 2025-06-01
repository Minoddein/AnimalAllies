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

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;

public class GetSpeciesWithPaginationHandlerDapper : IQueryHandler<PagedList<SpeciesDto>, GetSpeciesWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetSpeciesWithPaginationQuery> _validator;
    private readonly ILogger<GetSpeciesWithPaginationHandlerDapper> _logger;
    private readonly HybridCache _hybridCache;

    public GetSpeciesWithPaginationHandlerDapper(
        [FromKeyedServices(Constraints.Context.BreedManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetSpeciesWithPaginationHandlerDapper> logger,
        IValidator<GetSpeciesWithPaginationQuery> validator,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
        _hybridCache = hybridCache;
    }

    public async Task<Result<PagedList<SpeciesDto>>> Handle(GetSpeciesWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(3),
            LocalCacheExpiration = TimeSpan.FromMinutes(60)
        };

        //TODO:Сделать рефакторинг: добавить запрос на получение общего числа записей и добавлять его в TotalCount,
        //сделать это для всех запросов

        var cachedSpecies = await _hybridCache.GetOrCreateAsync(
            key: $"{TagsConstants.SPECIES}_{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();
                var parameters = new DynamicParameters();
                
                var speciesSql = new StringBuilder("""
                                                   SELECT 
                                                       s.id as species_id,
                                                       s.name as species_name
                                                   FROM species.species s
                                                   """);

                speciesSql.ApplySorting(query.SortBy, query.SortDirection);
                speciesSql.ApplyPagination(query.Page, query.PageSize);

                var pagedSpecies = (await connection.QueryAsync<SpeciesDto>(speciesSql.ToString())).ToList();

                if (!pagedSpecies.Any())
                    return [];
                
                var speciesIds = pagedSpecies.Select(s => s.SpeciesId).ToList();

                var breedsSql = """
                                SELECT 
                                    b.id as breed_id,
                                    b.name as breed_name,
                                    b.species_id as species_id
                                FROM species.breeds b
                                WHERE b.species_id = ANY(@SpeciesIds) AND b.is_deleted = false
                                """;

                var breeds = await connection.QueryAsync<BreedDto>(breedsSql, new { SpeciesIds = speciesIds });
                
                var breedsLookup = breeds.GroupBy(b => b.SpeciesId)
                    .ToDictionary(g => g.Key, g => g.ToArray());

                return pagedSpecies.Select(s =>
                {
                    s.Breeds = breedsLookup.TryGetValue(s.SpeciesId, out var speciesBreeds) ? speciesBreeds : [];
                    return s;
                });
            },
            options: options,
            tags: [TagsConstants.SPECIES],
            cancellationToken: cancellationToken);


        _logger.LogInformation("Get species with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var speciesDtos = cachedSpecies.ToList();

        return new PagedList<SpeciesDto>
        {
            Items = speciesDtos.ToList(),
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = speciesDtos.Count
        };
    }

    public async Task<Result<List<SpeciesDto>>> Handle(CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();


        var sql = new StringBuilder("""
                                    select 
                                        id,
                                        name
                                        from species.species
                                    """);


        var species = await connection.QueryAsync<SpeciesDto>(sql.ToString());

        _logger.LogInformation("Get species with pagination Page");


        return species.ToList();
    }
}