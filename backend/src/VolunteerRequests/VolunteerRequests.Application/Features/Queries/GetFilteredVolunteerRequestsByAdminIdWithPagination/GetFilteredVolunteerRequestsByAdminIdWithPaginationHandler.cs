using System.Data;
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
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByAdminIdWithPagination;

public class GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler(
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    ISqlConnectionFactory sqlConnectionFactory,
    IValidator<GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery> validator,
    ILogger<GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler> logger,
    HybridCache hybridCache) :
    IQueryHandler<PagedList<VolunteerRequestDto>, GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        string cacheKey = $"{TagsConstants.VOLUNTEER_REQUESTS}_{query.AdminId}" +
                          $":status_{query.RequestStatus}" +
                          $":sort_{query.SortBy}" +
                          $":dir_{query.SortDirection}" +
                          $":page_{query.Page}" +
                          $":size_{query.PageSize}";

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(3), LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        List<VolunteerRequestDto> result = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();
                DynamicParameters parameters = new();
                parameters.Add("@AdminId", query.AdminId);

                StringBuilder sql = new("""
                                        SELECT 
                                            id, first_name, second_name, patronymic,
                                            description, email, phone_number, work_experience,
                                            admin_id, user_id, discussion_id, request_status,
                                            rejection_comment, social_networks
                                        FROM volunteer_requests.volunteer_requests 
                                        WHERE admin_id = @AdminId
                                        """);

                bool hasWhereClause = true;
                Dictionary<string, string> stringProperties = new() { { "request_status", query.RequestStatus } };
                sql.ApplyFilterByString(ref hasWhereClause, stringProperties);

                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page, query.PageSize);

                IEnumerable<VolunteerRequestDto> requests =
                    await connection.QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                        sql.ToString(),
                        (volunteerRequest, socialNetworks) =>
                        {
                            volunteerRequest.SocialNetworks = socialNetworks;
                            return volunteerRequest;
                        },
                        splitOn: "social_networks",
                        param: parameters).ConfigureAwait(false);

                List<VolunteerRequestDto> list = [.. requests];

                return list;
            },
            options,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_ADMIN + "_" + query.AdminId)
            ],
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Get volunteer requests with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        return new PagedList<VolunteerRequestDto>
        {
            Items = result, PageSize = query.PageSize, Page = query.Page, TotalCount = result.Count
        };
    }
}