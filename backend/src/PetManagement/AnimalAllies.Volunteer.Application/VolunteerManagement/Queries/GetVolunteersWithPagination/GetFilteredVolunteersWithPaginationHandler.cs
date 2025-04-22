using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
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
        [FromKeyedServices(Constraints.Context.PetManagement)]ISqlConnectionFactory sqlConnectionFactory,
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
        
        var sql = new StringBuilder("""
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
        
        var stringProperties = new Dictionary<string, string>
        {
            { "first_name", query.FirstName },
            { "second_name", query.SecondName },
            { "patronymic", query.Patronymic },
        };

        sql.ApplyFilterByString(ref hasWhereClause,stringProperties);
        
        switch (query)
        {
            case { WorkExperienceFrom: not null, WorkExperienceTo: not null }:
                sql.ApplyBetweenFilter(ref hasWhereClause,"work_experience", (int)query.WorkExperienceFrom, (int)query.WorkExperienceTo);
                break;
            case {WorkExperienceFrom: not null, WorkExperienceTo: null}:
                sql.ApplyFilterByValueFrom(ref hasWhereClause,"work_experience", (int)query.WorkExperienceFrom);
                break;
            case {WorkExperienceFrom: null, WorkExperienceTo: not null}:
                sql.ApplyFilterByValueTo<int>(ref hasWhereClause,"work_experience", (int)query.WorkExperienceTo);
                break;
        }
        
        sql.ApplySorting(query.SortBy,query.SortDirection);
        
        sql.ApplyPagination(query.Page,query.PageSize);
        
        var volunteers = 
            await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
                sql.ToString(),
                (volunteer, requisites) =>
                {
                    volunteer.Requisites = requisites;
                    
                    return volunteer;
                },
                splitOn:"requisites",
                param: parameters);
        
        _logger.LogInformation("Get volunteers with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var volunteerDtos = volunteers.ToList();
        
        return new PagedList<VolunteerDto>
        {
            Items = volunteerDtos.ToList(),
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = volunteerDtos.Count()
        };
    }
    
}