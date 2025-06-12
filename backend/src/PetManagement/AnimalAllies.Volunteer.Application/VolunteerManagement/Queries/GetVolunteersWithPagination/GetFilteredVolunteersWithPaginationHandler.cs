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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteersWithPagination;

public class GetFilteredVolunteersWithPaginationHandler :
    IQueryHandler<PagedList<VolunteerDto>, GetFilteredVolunteersWithPaginationQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetFilteredVolunteersWithPaginationQuery> _validator;
    private readonly ILogger<GetFilteredVolunteersWithPaginationHandler> _logger;

    public GetFilteredVolunteersWithPaginationHandler(
        IReadDbContext readDbContext,
        IValidator<GetFilteredVolunteersWithPaginationQuery> validator,
        ILogger<GetFilteredVolunteersWithPaginationHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<PagedList<VolunteerDto>>> Handle(
        GetFilteredVolunteersWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (validationResult.IsValid == false)
            validationResult.ToErrorList();

        var volunteerQuery = _readDbContext.Volunteers;

        volunteerQuery = VolunteerQueryFilter(query, volunteerQuery);

        var keySelector = SortByProperty(query);

        volunteerQuery = query.SortDirection?.ToLower() == "desc"
            ? volunteerQuery.OrderByDescending(keySelector)
            : volunteerQuery.OrderBy(keySelector);

        var pagedList = await volunteerQuery.ToPagedList(
            query.Page,
            query.PageSize,
            cancellationToken);

        _logger.LogInformation(
            "Get volunteers with pagination Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            pagedList.Page, pagedList.PageSize, pagedList.TotalCount);

        return pagedList;
    }

    private static Expression<Func<VolunteerDto, object>> SortByProperty(GetFilteredVolunteersWithPaginationQuery query)
    {
        Expression<Func<VolunteerDto, object>> keySelector = query.SortBy?.ToLower() switch
        {
            "name" => (volunteer) => volunteer.FirstName,
            "surname" => (volunteer) => volunteer.SecondName,
            "patronymic" => (volunteer) => volunteer.Patronymic,
            "age" => (volunteer) => volunteer.WorkExperience,
            _ => (volunteer) => volunteer.Id
        };
        return keySelector;
    }

    private static IQueryable<VolunteerDto> VolunteerQueryFilter(GetFilteredVolunteersWithPaginationQuery query,
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

public class GetFilteredVolunteersWithPaginationHandlerDapper :
    IQueryHandler<PagedList<VolunteerDto>, GetFilteredVolunteersWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetFilteredVolunteersWithPaginationHandlerDapper> _logger;

    public GetFilteredVolunteersWithPaginationHandlerDapper(
        [FromKeyedServices(Constraints.Context.PetManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetFilteredVolunteersWithPaginationHandlerDapper> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<PagedList<VolunteerDto>>> Handle(
        GetFilteredVolunteersWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        var totalCountSql = new StringBuilder("""
                                           SELECT COUNT(*) FROM volunteers.volunteers WHERE is_deleted = false
                                           """);

        var sql = new StringBuilder("""
                                    select 
                                        v.id,
                                        relation_id as user_id,
                                        first_name,
                                        second_name,
                                        patronymic,
                                        description,
                                        v.email,
                                        v.phone_number,
                                        work_experience,
                                        u.photo as avatar_url,
                                        v.requisites,
                                        COUNT(p.id) as animal_count
                                        from volunteers.volunteers v
                                        inner join accounts.users u on u.Id = relation_id
                                        left join volunteers.pets p on v.id = p.volunteer_id and p.is_deleted = false
                                            where v.is_deleted = false
                                    """);
        
        
        var hasWhereClause = true;

        switch (query)
        {
            case { WorkExperienceFrom: not null, WorkExperienceTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom,
                    (int)query.WorkExperienceTo);
                totalCountSql.ApplyBetweenFilter(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom,
                    (int)query.WorkExperienceTo);
                break;
            case { WorkExperienceFrom: not null, WorkExperienceTo: null }:
                sql.ApplyFilterByValueFrom(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom);
                totalCountSql.ApplyFilterByValueFrom(ref hasWhereClause, "work_experience", (int)query.WorkExperienceFrom);
                break;
            case { WorkExperienceFrom: null, WorkExperienceTo: not null }:
                sql.ApplyFilterByValueTo(ref hasWhereClause, "work_experience", (int)query.WorkExperienceTo);
                totalCountSql.ApplyFilterByValueTo(ref hasWhereClause, "work_experience", (int)query.WorkExperienceTo);
                break;
        }

        if (query.SearchTerm is not null)
        {
            var stringProperties = new Dictionary<string, string>
            {
                { "first_name", query.SearchTerm },
                { "second_name", query.SearchTerm },
                { "patronymic", query.SearchTerm },
            };
            
            sql.ApplySearchByEqualsValue(ref  hasWhereClause, stringProperties);
            totalCountSql.ApplySearchByEqualsValue(ref hasWhereClause, stringProperties);
        }
        

        sql.Append("""
                   
                   group by 
                   v.id,
                   relation_id,
                   first_name,
                   second_name,
                   patronymic,
                   description,
                   v.email,
                   v.phone_number,
                   work_experience,
                   u.photo,
                   v.requisites
                   """);
        
        sql.ApplySorting(query.SortBy, query.SortDirection);

        sql.ApplyPagination(query.Page, query.PageSize);
        
        var s = totalCountSql.ToString();

        var volunteers =
            await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
                sql.ToString(),
                (volunteer, requisites) =>
                {
                    volunteer.Requisites = requisites;

                    return volunteer;
                },
                splitOn: "requisites",
                param: parameters);

        var totalCount = await connection.ExecuteScalarAsync<int>(totalCountSql.ToString(), param: parameters);
        
        _logger.LogInformation("Get volunteers with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var volunteerDtos = volunteers.ToList();

        return new PagedList<VolunteerDto>
        {
            Items = volunteerDtos.ToList(),
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = totalCount
        };
    }
}