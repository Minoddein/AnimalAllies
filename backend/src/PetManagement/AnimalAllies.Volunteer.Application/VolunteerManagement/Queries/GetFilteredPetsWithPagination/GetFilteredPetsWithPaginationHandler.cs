using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetFilteredPetsWithPagination;

public class GetFilteredPetsWithPaginationHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetFilteredPetsWithPaginationHandler> logger,
    IValidator<GetFilteredPetsWithPaginationQuery> validator)
    : IQueryHandler<PagedList<PetDto>, GetFilteredPetsWithPaginationQuery>
{
    private readonly ILogger<GetFilteredPetsWithPaginationHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetFilteredPetsWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<PetDto>>> Handle(
        GetFilteredPetsWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@VolunteerId", query.VolunteerId);

        StringBuilder sql = new("""
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
                                    requisites,
                                    pet_photos
                                    from volunteers.pets
                                    where volunteer_id = @VolunteerId and 
                                          is_deleted = false
                                """);

        bool hasWhereClause = true;

        Dictionary<string, string> stringProperties = new()
        {
            { "name", query.Name },
            { "city", query.City },
            { "state", query.State },
            { "street", query.Street },
            { "zip_code", query.ZipCode },
            { "breed_id", query.BreedId.ToString() },
            { "species_id", query.SpeciesId.ToString() },
            { "help_status", query.HelpStatus },
            { "color", query.Color }
        };

        sql.ApplyFilterByString(ref hasWhereClause, stringProperties);

        FilterByValue(ref hasWhereClause, query, sql);

        sql.ApplySorting(query.SortBy, query.SortDirection);

        sql.ApplyPagination(query.Page, query.PageSize);

        IEnumerable<PetDto> pets =
            await connection.QueryAsync<PetDto, RequisiteDto[], PetPhotoDto[], PetDto>(
                sql.ToString(),
                (pet, requisites, petPhotoDtos) =>
                {
                    pet.Requisites = requisites;

                    pet.PetPhotos = petPhotoDtos;

                    return pet;
                },
                splitOn: "requisites, pet_photos",
                param: parameters).ConfigureAwait(false);

        _logger.LogInformation(
            "Get pets with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        List<PetDto> petDtos = [.. pets];

        return new PagedList<PetDto>
        {
            Items = petDtos, PageSize = query.PageSize, Page = query.Page, TotalCount = petDtos.Count
        };
    }

    private static void FilterByValue(ref bool hasWhereClause, GetFilteredPetsWithPaginationQuery query,
        StringBuilder sql)
    {
        switch (query)
        {
            case { PositionFrom: not null, PositionTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "position", (int)query.PositionFrom, (int)query.PositionTo);
                break;
            case { PositionFrom: not null, PositionTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "position", (int)query.PositionFrom);
                break;
            case { PositionFrom: null, PositionTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "position", (int)query.PositionTo);
                break;
        }

        switch (query)
        {
            case { WeightFrom: not null, WeightTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "weight", (int)query.WeightFrom, (int)query.WeightTo);
                break;
            case { WeightFrom: not null, WeightTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "weight", (int)query.WeightFrom);
                break;
            case { WeightFrom: null, WeightTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "weight", (int)query.WeightTo);
                break;
        }

        switch (query)
        {
            case { HeightFrom: not null, HeightTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "height", (int)query.HeightFrom, (int)query.HeightTo);
                break;
            case { HeightFrom: not null, HeightTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "height", (int)query.HeightFrom);
                break;
            case { HeightFrom: null, HeightTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "height", (int)query.HeightTo);
                break;
        }

        switch (query)
        {
            case { BirthDateFrom: not null, BirthDateTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "birth_date",
                    DateOnly.FromDateTime((DateTime)query.BirthDateFrom),
                    DateOnly.FromDateTime((DateTime)query.BirthDateTo));
                break;
            case { BirthDateFrom: not null, BirthDateTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "birth_date",
                    DateOnly.FromDateTime((DateTime)query.BirthDateFrom));
                break;
            case { BirthDateFrom: null, BirthDateTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "birth_date",
                    DateOnly.FromDateTime((DateTime)query.BirthDateTo));
                break;
        }

        if (query.IsCastrated is not null)
        {
            sql.ApplyFilterByEqualsValue(ref hasWhereClause, "is_castrated", query.IsCastrated);
        }

        if (query.IsVaccinated is not null)
        {
            sql.ApplyFilterByEqualsValue(ref hasWhereClause, "is_vaccinated", query.IsVaccinated);
        }
    }
}