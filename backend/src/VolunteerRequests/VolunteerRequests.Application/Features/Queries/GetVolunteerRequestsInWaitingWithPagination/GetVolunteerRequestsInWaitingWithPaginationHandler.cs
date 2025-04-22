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
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;

public class GetVolunteerRequestsInWaitingWithPaginationHandler:
    IQueryHandler<PagedList<VolunteerRequestDto>, GetVolunteerRequestsInWaitingWithPaginationQuery>
{
    private const string REDIS_KEY = "volunteer-requests_";
    
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetVolunteerRequestsInWaitingWithPaginationQuery> _validator;
    private readonly ILogger<GetVolunteerRequestsInWaitingWithPaginationHandler> _logger;
    private readonly HybridCache _hybridCache;
    
    public GetVolunteerRequestsInWaitingWithPaginationHandler(
        IValidator<GetVolunteerRequestsInWaitingWithPaginationQuery> validator,
        ILogger<GetVolunteerRequestsInWaitingWithPaginationHandler> logger,
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]ISqlConnectionFactory sqlConnectionFactory,
        HybridCache hybridCache)
    {
        _validator = validator;
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
        _hybridCache = hybridCache;
    }
    
    public async Task<Result<PagedList<VolunteerRequestDto>>> Handle(
        GetVolunteerRequestsInWaitingWithPaginationQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(1)
        };
        
        var cachedVolunteerRequests = await _hybridCache.GetOrCreateAsync(
            key: $"{REDIS_KEY}{query.Page}_{query.PageSize}_{query.SortBy}_{query.SortDirection}",
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
                parameters.Add("@RequestStatus", RequestStatus.Waiting.Value);
        
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
                                                where request_status = @RequestStatus
                                            """);
                
                sql.ApplySorting(query.SortBy,query.SortDirection);
        
                sql.ApplyPagination(query.Page,query.PageSize);
                
                var result = await connection
                    .QueryAsync<VolunteerRequestDto, SocialNetworkDto[], VolunteerRequestDto>(
                    sql.ToString(),
                    (volunteerRequest, socialNetworks) =>
                    {
                        volunteerRequest.SocialNetworks = socialNetworks;
                        return volunteerRequest;
                    },
                    splitOn:"social_networks",
                    param: parameters);

                return result;
            },
            options: options,
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Get volunteer requests with pagination Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var volunteerRequestDtos = cachedVolunteerRequests.ToList();
        
        return new PagedList<VolunteerRequestDto>
        {
            Items = volunteerRequestDtos,
            PageSize = query.PageSize,
            Page = query.Page,
            TotalCount = volunteerRequestDtos.Count
        };
    }
}