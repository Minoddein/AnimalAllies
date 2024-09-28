﻿using System.Text;
using AnimalAllies.Application.Abstractions;
using AnimalAllies.Application.Contracts.DTOs;
using AnimalAllies.Application.Database;
using AnimalAllies.Application.Extension;
using AnimalAllies.Application.Features.Species.Queries.GetSpeciesWithPagination;
using AnimalAllies.Application.Models;
using AnimalAllies.Domain.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Application.Features.Species.Queries.GetBreedsBySpeciesId;

public class GetBreedsBySpeciesIdWithPaginationHandlerDapper: IQueryHandler<PagedList<BreedDto>, GetBreedsBySpeciesIdWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetBreedsBySpeciesIdWithPaginationQuery> _validator;
    private readonly ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> _logger;

    public GetBreedsBySpeciesIdWithPaginationHandlerDapper(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetBreedsBySpeciesIdWithPaginationHandlerDapper> logger,
        IValidator<GetBreedsBySpeciesIdWithPaginationQuery> validator)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<PagedList<BreedDto>>> Handle(GetBreedsBySpeciesIdWithPaginationQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        
        parameters.Add("@SpeciesId", query.SpeciesId);

        var sql = new StringBuilder("""
                                    select 
                                        id,
                                        name,
                                        species_id
                                        from breeds
                                    where species_id = @SpeciesId
                                    """);
        
        sql.ApplySorting(query.SortBy, query.SortDirection);
        sql.ApplyPagination(query.Page,query.PageSize);

        var breeds = await connection.QueryAsync<BreedDto>(sql.ToString(), parameters);
        
        _logger.LogInformation("Get breeds with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var breedsDtos = breeds.ToList();
        
        return new PagedList<BreedDto>
        {
            Items = breedsDtos.ToList(),
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = breedsDtos.Count()
        };

    }
}