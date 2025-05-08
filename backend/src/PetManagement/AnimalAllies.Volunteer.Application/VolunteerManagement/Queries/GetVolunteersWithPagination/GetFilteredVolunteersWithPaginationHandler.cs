using System.Data;
using System.Linq.Expressions;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Volunteer.Application.Database;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteersWithPagination;

public class GetFilteredVolunteersWithPaginationHandler(
    IReadDbContext readDbContext,
    IValidator<GetFilteredVolunteersWithPaginationQuery> validator,
    ILogger<GetFilteredVolunteersWithPaginationHandler> logger) :
    IQueryHandler<PagedList<VolunteerDto>, GetFilteredVolunteersWithPaginationQuery>
{
    private readonly ILogger<GetFilteredVolunteersWithPaginationHandler> _logger = logger;
    private readonly IReadDbContext _readDbContext = readDbContext;
    private readonly IValidator<GetFilteredVolunteersWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<VolunteerDto>>> Handle(
        GetFilteredVolunteersWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (validationResult.IsValid == false)
        {
            validationResult.ToErrorList();
        }

        IQueryable<VolunteerDto> volunteerQuery = _readDbContext.Volunteers;

        volunteerQuery = VolunteerQueryFilter(query, volunteerQuery);

        Expression<Func<VolunteerDto, object>> keySelector = SortByProperty(query);

        volunteerQuery = query.SortDirection?.ToLower() == "desc"
            ? volunteerQuery.OrderByDescending(keySelector)
            : volunteerQuery.OrderBy(keySelector);

        PagedList<VolunteerDto> pagedList = await volunteerQuery.ToPagedList(
            query.Page,
            query.PageSize,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Get volunteers with pagination Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            pagedList.Page, pagedList.PageSize, pagedList.TotalCount);

        return pagedList;
    }

    private static Expression<Func<VolunteerDto, object>> SortByProperty(GetFilteredVolunteersWithPaginationQuery query)
    {
        Expression<Func<VolunteerDto, object>> keySelector = query.SortBy?.ToLower() switch
        {
            "name" => volunteer => volunteer.FirstName,
            "surname" => volunteer => volunteer.SecondName,
            "patronymic" => volunteer => volunteer.Patronymic,
            "age" => volunteer => volunteer.WorkExperience,
            _ => volunteer => volunteer.Id
        };
        return keySelector;
    }

    private static IQueryable<VolunteerDto> VolunteerQueryFilter(
        GetFilteredVolunteersWithPaginationQuery query,
        IQueryable<VolunteerDto> volunteerQuery)
    {
        volunteerQuery = volunteerQuery.WhereIf(
            !string.IsNullOrWhiteSpace(query.FirstName),
            v => v.FirstName.Contains(query.FirstName!));

        volunteerQuery = volunteerQuery.WhereIf(
            !string.IsNullOrWhiteSpace(query.SecondName),
            v => v.SecondName.Contains(query.SecondName!));

        volunteerQuery = volunteerQuery.WhereIf(
            !string.IsNullOrWhiteSpace(query.Patronymic),
            v => v.Patronymic.Contains(query.Patronymic!));

        volunteerQuery = volunteerQuery.WhereIf(
            query.WorkExperienceFrom != null,
            v => v.WorkExperience >= query.WorkExperienceFrom);

        volunteerQuery = volunteerQuery.WhereIf(
            query.WorkExperienceTo != null,
            v => v.WorkExperience <= query.WorkExperienceTo);
        return volunteerQuery;
    }
}

public class GetFilteredVolunteersWithPaginationHandlerDapper(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetFilteredVolunteersWithPaginationHandlerDapper> logger) :
    IQueryHandler<PagedList<VolunteerDto>, GetFilteredVolunteersWithPaginationQuery>
{
    private readonly ILogger<GetFilteredVolunteersWithPaginationHandlerDapper> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<Result<PagedList<VolunteerDto>>> Handle(
        GetFilteredVolunteersWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        StringBuilder sql = new("""
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
                                        where is_deleted = false
                                """);

        bool hasWhereClause = true;

        Dictionary<string, string> stringProperties = new()
        {
            { "first_name", query.FirstName },
            { "second_name", query.SecondName },
            { "patronymic", query.Patronymic }
        };

        sql.ApplyFilterByString(ref hasWhereClause, stringProperties);

        switch (query)
        {
            case { WorkExperienceFrom: not null, WorkExperienceTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom,
                    (int)query.WorkExperienceTo);
                break;
            case { WorkExperienceFrom: not null, WorkExperienceTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom);
                break;
            case { WorkExperienceFrom: null, WorkExperienceTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "work_experience", (int)query.WorkExperienceTo);
                break;
        }

        sql.ApplySorting(query.SortBy, query.SortDirection);

        sql.ApplyPagination(query.Page, query.PageSize);

        IEnumerable<VolunteerDto> volunteers =
            await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
                sql.ToString(),
                (volunteer, requisites) =>
                {
                    volunteer.Requisites = requisites;

                    return volunteer;
                },
                splitOn: "requisites",
                param: parameters).ConfigureAwait(false);

        _logger.LogInformation(
            "Get volunteers with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        List<VolunteerDto> volunteerDtos = [.. volunteers];

        return new PagedList<VolunteerDto>
        {
            Items = [.. volunteerDtos],
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = volunteerDtos.Count
        };
    }
}