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

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByUserIdWithPagination;

public class GetFilteredVolunteerRequestsByUserIdWithPaginationHandler(
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    ISqlConnectionFactory sqlConnectionFactory,
    IValidator<GetFilteredVolunteerRequestsByUserIdWithPaginationQuery> validator,
    ILogger<GetFilteredVolunteerRequestsByUserIdWithPaginationHandler> logger,
    HybridCache hybridCache) :
    IQueryHandler<PagedList<VolunteerRequestDto>, GetFilteredVolunteerRequestsByUserIdWithPaginationQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetFilteredVolunteerRequestsByUserIdWithPaginationHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetFilteredVolunteerRequestsByUserIdWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetFilteredVolunteerRequestsByUserIdWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        string cacheKey = $"{TagsConstants.VOLUNTEER_REQUESTS}_{query.UserId}_status-{query.RequestStatus}_sort" +
                          $"-{query.SortBy}_" +
                          $"dir-{query.SortDirection}_page-{query.Page}_size-{query.PageSize}";

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(3), LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        List<VolunteerRequestDto> cachedVolunteerRequests = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();
                DynamicParameters parameters = new();
                parameters.Add("@UserId", query.UserId);

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
                    Dictionary<string, string> stringProperties = new() { { "request_status", query.RequestStatus } };
                    sql.ApplyFilterByString(ref hasWhereClause, stringProperties);
                }

                sql.ApplySorting(query.SortBy, query.SortDirection);
                sql.ApplyPagination(query.Page, query.PageSize);

                IEnumerable<VolunteerRequestDto> result =
                    await connection.QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                        sql.ToString(),
                        (volunteerRequest, socialNetworks) =>
                        {
                            volunteerRequest.SocialNetworks = socialNetworks;
                            return volunteerRequest;
                        },
                        splitOn: "social_networks",
                        param: parameters).ConfigureAwait(false);

                return result.ToList();
            },
            options,
            [
                new string(TagsConstants.VOLUNTEER_REQUESTS + "_" +
                           TagsConstants.VolunteerRequests.BY_USER + "_" + query.UserId)
            ],
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Get volunteer requests with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        List<VolunteerRequestDto> volunteerRequestDtos = [.. cachedVolunteerRequests];

        return new PagedList<VolunteerRequestDto>
        {
            Items = volunteerRequestDtos,
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = volunteerRequestDtos.Count
        };
    }
}