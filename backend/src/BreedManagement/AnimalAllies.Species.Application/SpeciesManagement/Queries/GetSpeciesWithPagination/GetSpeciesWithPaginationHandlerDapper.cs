using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
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
    private const string REDIS_KEY = "species_";
    
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetSpeciesWithPaginationQuery> _validator;
    private readonly ILogger<GetSpeciesWithPaginationHandlerDapper> _logger;
    private readonly HybridCache _hybridCache;

    public GetSpeciesWithPaginationHandlerDapper(
        [FromKeyedServices(Constraints.Context.BreedManagement)]ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetSpeciesWithPaginationHandlerDapper> logger,
        IValidator<GetSpeciesWithPaginationQuery> validator,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
        _hybridCache = hybridCache;
    }
    
    public async Task<Result<PagedList<SpeciesDto>>> Handle(GetSpeciesWithPaginationQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        var sql = new StringBuilder("""
                                    select 
                                        id,
                                        name
                                        from species.species
                                    """);
        
        sql.ApplySorting(query.SortBy, query.SortDirection);
        sql.ApplyPagination(query.Page,query.PageSize);
        
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(24)
        };
        
        //TODO:Сделать рефакторинг: добавить запрос на получение общего числа записей и добавлять его в TotalCount,
        //сделать это для всех запросов
        
        var cachedSpecies = await _hybridCache.GetOrCreateAsync(
            key: REDIS_KEY + query.GetHashCode(),
            factory: async _ => await connection.QueryAsync<SpeciesDto>(sql.ToString(), parameters),
            options: options,
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