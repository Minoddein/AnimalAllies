using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPaginationBySearchTerm;

public class GetSpeciesWithPaginationBySearchTermQueryHandler :
    IQueryHandler<PagedList<SpeciesDto>, GetSpeciesWithPaginationBySearchTermQuery>
{
    private readonly ILogger<GetSpeciesWithPaginationBySearchTermQueryHandler> _logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetSpeciesWithPaginationBySearchTermQuery> _validator;

    public GetSpeciesWithPaginationBySearchTermQueryHandler(
        ILogger<GetSpeciesWithPaginationBySearchTermQueryHandler> logger,
        [FromKeyedServices(Constraints.Context.BreedManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetSpeciesWithPaginationBySearchTermQuery> validator)
    {
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
    }

    public async Task<Result<PagedList<SpeciesDto>>> Handle(
        GetSpeciesWithPaginationBySearchTermQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        var speciesQueryBuilder = new StringBuilder();
        var totalCountByQueryBuilder = new StringBuilder();

        speciesQueryBuilder.Append("""
                                   select 
                                       s.id as species_id,
                                       s.name as species_name,
                                       b.id as breed_id,
                                       b.name as breed_name
                                   from species.species s
                                   left join species.breeds b on b.species_id = s.id 
                                   where s.name ilike @SearchTerm or b.name ilike @SearchTerm
                                   """);
        
        totalCountByQueryBuilder.Append("""
                                        select 
                                            count(distinct s.id)
                                        from species.species s
                                        left join species.breeds b on b.species_id = s.id 
                                        where s.name ilike @SearchTerm or b.name ilike @SearchTerm
                                        """);
        
        var searchTerm = $"%{query.SearchTerm}%";
        parameters.Add("@Page", query.Page);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@SearchTerm", searchTerm);
        
        speciesQueryBuilder.ApplyPagination(query.Page, query.PageSize);
        
        var speciesList = (await connection
            .QueryAsync<SpeciesDto>(speciesQueryBuilder.ToString(), parameters)).ToList();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(totalCountByQueryBuilder.ToString(), parameters);

        var breedsList = await GetBreedsForSpecies(
            connection, 
            speciesList.Select(s => s.SpeciesId).ToList());
        
        var breedsLookup = breedsList.ToLookup(b => b.SpeciesId);

        var result = speciesList.Select(s => new SpeciesDto
        {
            SpeciesId = s.SpeciesId,
            SpeciesName = s.SpeciesName,
            Breeds = breedsLookup[s.SpeciesId].ToArray()
        }).ToList();
        
        return new PagedList<SpeciesDto>
        {
            Items = result,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
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
}