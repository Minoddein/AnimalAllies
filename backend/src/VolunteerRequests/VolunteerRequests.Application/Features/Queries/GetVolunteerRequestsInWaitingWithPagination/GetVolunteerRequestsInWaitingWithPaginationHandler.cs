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
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;

public class GetVolunteerRequestsInWaitingWithPaginationHandler(
    IValidator<GetVolunteerRequestsInWaitingWithPaginationQuery> validator,
    ILogger<GetVolunteerRequestsInWaitingWithPaginationHandler> logger,
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    ISqlConnectionFactory sqlConnectionFactory,
    HybridCache hybridCache) :
    IQueryHandler<PagedList<VolunteerRequestDto>, GetVolunteerRequestsInWaitingWithPaginationQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetVolunteerRequestsInWaitingWithPaginationHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetVolunteerRequestsInWaitingWithPaginationQuery> _validator = validator;

    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetVolunteerRequestsInWaitingWithPaginationQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(1), LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        IEnumerable<VolunteerRequestDto> cachedVolunteerRequests = await _hybridCache.GetOrCreateAsync(
            $"{TagsConstants.VOLUNTEER_REQUESTS}_{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();
                parameters.Add("@RequestStatus", RequestStatus.Waiting.Value);

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
                                            where request_status = @RequestStatus
                                        """);

                sql.ApplySorting(query.SortBy, query.SortDirection);

                sql.ApplyPagination(query.Page, query.PageSize);

                IEnumerable<VolunteerRequestDto> result = await connection
                    .QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                        sql.ToString(),
                        (volunteerRequest, socialNetworks) =>
                        {
                            volunteerRequest.SocialNetworks = socialNetworks;
                            return volunteerRequest;
                        },
                        splitOn: "social_networks",
                        param: parameters).ConfigureAwait(false);

                return result;
            },
            options,
            [new string(TagsConstants.VOLUNTEER_REQUESTS + "_" + TagsConstants.VolunteerRequests.IN_WAITING)],
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