using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestInWaitingCount;

public class GetVolunteerRequestInWaitingCountHandler: IQueryHandler<int, GetVolunteerRequestInWaitingCountQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetVolunteerRequestInWaitingCountHandler> _logger;

    public GetVolunteerRequestInWaitingCountHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetVolunteerRequestInWaitingCountHandler> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        GetVolunteerRequestInWaitingCountQuery query, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();
        var parameters = new DynamicParameters();
        parameters.Add("@RequestStatus", RequestStatus.Waiting.Value);
        var totalCount = await connection
            .ExecuteScalarAsync<int>(
                "select count(id) from volunteer_requests.volunteer_requests " +
                "where request_status = @RequestStatus",
                param: parameters);
        
        _logger.LogInformation("Got volunteer requests in waiting count");

        return totalCount;
    }
}