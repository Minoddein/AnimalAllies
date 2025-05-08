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

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetBreedsBySpeciesId;

public class
    GetBreedsBySpeciesIdWithPaginationHandlerDapper(
        [FromKeyedServices(Constraints.Context.BreedManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> logger,
        IValidator<GetBreedsBySpeciesIdWithPaginationQuery> validator,
        HybridCache hybridCache) : IQueryHandler<PagedList<BreedDto>,
    GetBreedsBySpeciesIdWithPaginationQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetBreedsBySpeciesIdWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<BreedDto>>> Handle(
        GetBreedsBySpeciesIdWithPaginationQuery query,
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

        IEnumerable<BreedDto> cachedBreeds = await _hybridCache.GetOrCreateAsync(
            $"{TagsConstants.BREEDS}_{query.SpeciesId}_{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();

                parameters.Add("@SpeciesId", query.SpeciesId);

                StringBuilder sql = new("""
                                        select 
                                            id,
                                            name,
                                            species_id
                                            from species.breeds
                                        where species_id = @SpeciesId
                                        """);

                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page, query.PageSize);

                return await connection.QueryAsync<BreedDto>(sql.ToString(), parameters).ConfigureAwait(false);
            },
            options,
            [new string(TagsConstants.BREEDS + "_" + query.SpeciesId)],
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Get breeds with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        List<BreedDto> breedsDtos = [.. cachedBreeds];

        return new PagedList<BreedDto>
        {
            Items = [.. breedsDtos], PageSize = query.PageSize, Page = query.Page, TotalCount = breedsDtos.Count
        };
    }

    public async Task<Result<List<BreedDto>>> Handle(Guid speciesId, CancellationToken cancellationToken = default)
    {
        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@SpeciesId", speciesId);

        StringBuilder sql = new("""
                                select 
                                    id,
                                    name,
                                    species_id
                                    from species.breeds
                                where species_id = @SpeciesId
                                """);

        IEnumerable<BreedDto> breeds =
            await connection.QueryAsync<BreedDto>(sql.ToString(), parameters).ConfigureAwait(false);

        _logger.LogInformation("Get breeds with pagination with {speciesId}", speciesId);

        return breeds.ToList();
    }
}