using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByAdminIdWithPagination;

public class GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler:
    IQueryHandler<PagedList<VolunteerRequestDto>, GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery> _validator;
    private readonly ILogger<GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler> _logger;
    private readonly HybridCache _hybridCache;
    
    public GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery> validator, 
        ILogger<GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler> logger,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
        _logger = logger;
        _hybridCache = hybridCache;
    }

    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();
        
        var cacheKey = $"{TagsConstants.VOLUNTEER_REQUESTS}_{query.AdminId}" +
                       $":status_{query.RequestStatus}" +
                       $":sort_{query.SortBy}" +
                       $":dir_{query.SortDirection}" +
                       $":page_{query.Page}" +
                       $":size_{query.PageSize}";

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(3),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };
        var connection = _sqlConnectionFactory.Create();
        var parameters = new DynamicParameters();
        parameters.Add("@AdminId", query.AdminId);
        
        var totalCount = await connection
            .ExecuteScalarAsync<int>(
                "select count(id) from volunteer_requests.volunteer_requests " +
                "where admin_id = @AdminId",
                param: parameters);
        
        var result = await _hybridCache.GetOrCreateAsync(
            key:cacheKey,
            factory:async _ =>
            {
                var sql = new StringBuilder("""
                    SELECT 
                        id, first_name, second_name, patronymic, created_at,
                        description as volunteer_description, email, phone_number, work_experience,
                        admin_id, user_id, discussion_id, request_status,
                        rejection_comment, social_networks
                    FROM volunteer_requests.volunteer_requests 
                    WHERE admin_id = @AdminId
                    """);

                var hasWhereClause = true;
                var stringProperties = new Dictionary<string, string> { { "request_status", query.RequestStatus } };
                sql.ApplyFilterByString(ref hasWhereClause, stringProperties);
                
                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page, query.PageSize);

                var requests = await connection.QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                    sql.ToString(),
                    (volunteerRequest, socialNetworks) =>
                    {
                        volunteerRequest.SocialNetworks = socialNetworks;
                        return volunteerRequest;
                    },
                    splitOn: "social_networks",
                    param: parameters);

                var list = requests.ToList();

                return list;
            },
            options:options,
            tags: [new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + 
                              TagsConstants.VolunteerRequests.BY_ADMIN + "_" + query.AdminId)],
            cancellationToken: cancellationToken);
        
        
        
        _logger.LogInformation("Get volunteer requests with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);
        
        return new PagedList<VolunteerRequestDto>
        {
            Items = result,
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = totalCount
        };
    }
}