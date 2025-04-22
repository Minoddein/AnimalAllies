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
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByUserIdWithPagination;

public class GetFilteredVolunteerRequestsByUserIdWithPaginationHandler:
    IQueryHandler<PagedList<VolunteerRequestDto>, GetFilteredVolunteerRequestsByUserIdWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetFilteredVolunteerRequestsByUserIdWithPaginationQuery> _validator;
    private readonly ILogger<GetFilteredVolunteerRequestsByUserIdWithPaginationHandler> _logger;

    public GetFilteredVolunteerRequestsByUserIdWithPaginationHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetFilteredVolunteerRequestsByUserIdWithPaginationQuery> validator, 
        ILogger<GetFilteredVolunteerRequestsByUserIdWithPaginationHandler> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetFilteredVolunteerRequestsByUserIdWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();
        
        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", query.UserId);
        
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
                                        admin_id,
                                        user_id,
                                        discussion_id,
                                        request_status,
                                        rejection_comment,
                                        social_networks
                                        from volunteer_requests.volunteer_requests 
                                        where user_id = @UserId
                                    """);
        bool hasWhereClause = true;

        if (query.RequestStatus != null)
        {
            var stringProperties = new Dictionary<string, string>(){ { "request_status", query.RequestStatus } };
        
            sql.ApplyFilterByString(ref hasWhereClause, stringProperties);
        }

        sql.ApplySorting(query.SortBy,query.SortDirection);
        
        sql.ApplyPagination(query.Page,query.PageSize);
        
        var volunteerRequests = 
            await connection.QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                sql.ToString(),
                (volunteerRequest, socialNetworks) =>
                {
                    volunteerRequest.SocialNetworks = socialNetworks;
                    return volunteerRequest;
                },
                splitOn:"social_networks",
                param: parameters);
        
        _logger.LogInformation("Get volunteer requests with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var volunteerRequestDtos = volunteerRequests.ToList();
        
        return new PagedList<VolunteerRequestDto>
        {
            Items = volunteerRequestDtos,
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = volunteerRequestDtos.Count()
        };
    }
}