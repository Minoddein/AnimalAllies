using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUsersCount;

public class GetUsersCountHandler : IQueryHandler<UsersCountResponse, GetUsersCountQuery>
{
    private readonly ILogger<GetUsersCountHandler> _logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUsersCountHandler(
        ILogger<GetUsersCountHandler> logger,
        [FromKeyedServices(Constraints.Context.Accounts)]
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<UsersCountResponse>> Handle(
        GetUsersCountQuery query, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.Create();

        var sql = """
                  SELECT 
                      COUNT(*) AS TotalUsers,
                      SUM(CASE WHEN email_confirmed = true THEN 1 ELSE 0 END) AS ActiveUsers,
                      SUM(CASE WHEN volunteer_account_id IS NOT NULL THEN 1 ELSE 0 END) AS VolunteerUsers
                  FROM accounts.users
                  """;

        var result = await connection.QueryFirstAsync<UsersCountResponse>(sql);

        _logger.LogInformation("Successfully retrieved users count");

        return result;
    }
}