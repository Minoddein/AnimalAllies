using System.Data;
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

        var connection = _sqlConnectionFactory.Create();
        var cacheKey =
            $"{TagsConstants.SPECIES}_{query.SortBy}_{query.SortDirection}_{query.SearchTerm}_{query.Page}_{query.PageSize}";

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(3),
            LocalCacheExpiration = TimeSpan.FromMinutes(60)
        };

        var pagedSpecies = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ => await LoadAllSpecies(connection, query),
            options,
            [TagsConstants.SPECIES],
            cancellationToken);

        var totalCount = await CountFilteredSpecies(connection, query);

        return new PagedList<SpeciesDto>
        {
            Items = pagedSpecies,
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = totalCount
        };
    }


    private async Task<List<SpeciesDto>> LoadAllSpecies(IDbConnection connection, GetSpeciesWithPaginationQuery query)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();

        sqlBuilder.Append("""
                          SELECT DISTINCT ON (s.id)
                              s.id as species_id,
                              s.name as species_name
                          FROM species.species s
                          LEFT JOIN species.breeds b ON b.species_id = s.id
                          """);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            sqlBuilder.Append("""
                              WHERE s.name ILIKE @SearchTerm OR b.name ILIKE @SearchTerm
                              """);
            parameters.Add("SearchTerm", $"%{query.SearchTerm}%");
        }

        sqlBuilder.ApplySorting(query.SortBy, query.SortDirection, "s.name"); // fallback sort
        sqlBuilder.Append(" LIMIT @Limit OFFSET @Offset");
        parameters.Add("Limit", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        var species = (await connection.QueryAsync<SpeciesDto>(sqlBuilder.ToString(), parameters)).ToList();

        if (!species.Any())
            return [];

        var breeds = await GetBreedsForSpecies(connection, species.Select(s => s.SpeciesId).ToList());
        var breedsLookup = breeds.ToLookup(b => b.SpeciesId);

        return species.Select(s => new SpeciesDto
        {
            SpeciesId = s.SpeciesId,
            SpeciesName = s.SpeciesName,
            Breeds = breedsLookup[s.SpeciesId].ToArray()
        }).ToList();
    }

    private async Task<int> CountFilteredSpecies(IDbConnection connection, GetSpeciesWithPaginationQuery query)
    {
        var countSql = new StringBuilder("""
                                         SELECT COUNT(DISTINCT s.id)
                                         FROM species.species s
                                         LEFT JOIN species.breeds b ON b.species_id = s.id
                                         """);

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            countSql.Append("""
                            WHERE s.name ILIKE @SearchTerm OR b.name ILIKE @SearchTerm
                            """);
            parameters.Add("SearchTerm", $"%{query.SearchTerm}%");
        }

        return await connection.ExecuteScalarAsync<int>(countSql.ToString(), parameters);
    }

    private async Task<List<BreedDto>> GetBreedsForSpecies(IDbConnection connection, List<Guid> speciesIds)
    {
        const string sql = """
                           SELECT 
                               id as breed_id,
                               name as breed_name,
                               species_id
                           FROM species.breeds
                           WHERE species_id = ANY(@SpeciesIds)
                           """;

        return (await connection.QueryAsync<BreedDto>(sql, new { SpeciesIds = speciesIds }))
            .ToList();
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