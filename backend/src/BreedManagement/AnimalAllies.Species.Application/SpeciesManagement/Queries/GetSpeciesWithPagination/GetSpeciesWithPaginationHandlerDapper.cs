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
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;

public class GetSpeciesWithPaginationHandlerDapper(
    [FromKeyedServices(Constraints.Context.BreedManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetSpeciesWithPaginationHandlerDapper> logger,
    IValidator<GetSpeciesWithPaginationQuery> validator,
    HybridCache hybridCache) : IQueryHandler<PagedList<SpeciesDto>, GetSpeciesWithPaginationQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetSpeciesWithPaginationHandlerDapper> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetSpeciesWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<SpeciesDto>>> Handle(
        GetSpeciesWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromHours(3), LocalCacheExpiration = TimeSpan.FromMinutes(60)
        };

        // TODO:Сделать рефакторинг: добавить запрос на получение общего числа записей и добавлять его в TotalCount,
        // сделать это для всех запросов
        IEnumerable<SpeciesDto> cachedSpecies = await _hybridCache.GetOrCreateAsync(
            $"{TagsConstants.SPECIES}_{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();

                StringBuilder sql = new("""
                                        select 
                                            id,
                                            name
                                            from species.species
                                        """);

                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page, query.PageSize);

                return await connection.QueryAsync<SpeciesDto>(sql.ToString(), parameters).ConfigureAwait(false);
            },
            options,
            [TagsConstants.SPECIES],
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Get species with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        List<SpeciesDto> speciesDtos = [.. cachedSpecies];

        return new PagedList<SpeciesDto>
        {
            Items = [.. speciesDtos], PageSize = query.PageSize, Page = query.Page, TotalCount = speciesDtos.Count
        };
    }

    public async Task<Result<List<SpeciesDto>>> Handle(CancellationToken cancellationToken = default)
    {
        IDbConnection connection = _sqlConnectionFactory.Create();

        StringBuilder sql = new("""
                                select 
                                    id,
                                    name
                                    from species.species
                                """);

        IEnumerable<SpeciesDto> species = await connection.QueryAsync<SpeciesDto>(sql.ToString()).ConfigureAwait(false);

        _logger.LogInformation("Get species with pagination Page");

        return species.ToList();
    }
}