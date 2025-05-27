using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestByRelationUser;

public class GetVolunteerRequestByRelationUserHandler: IQueryHandler<List<VolunteerRequestDto>,GetVolunteerRequestByRelationUserQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetVolunteerRequestByRelationUserQuery> _validator;
    private readonly ILogger<GetVolunteerRequestByRelationUserHandler> _logger;

    public GetVolunteerRequestByRelationUserHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetVolunteerRequestByRelationUserQuery> validator,
        ILogger<GetVolunteerRequestByRelationUserHandler> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<VolunteerRequestDto>>> Handle(
        GetVolunteerRequestByRelationUserQuery query, 
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
                                        admin_id,
                                        user_id,
                                        discussion_id,
                                        request_status
                                    from volunteer_requests.volunteer_requests 
                                    where user_id = @UserId or admin_id = @UserId
                                    """);
        

        var result = await connection.QueryAsync<VolunteerRequestDto>(
            sql.ToString(),
            param: parameters);

        _logger.LogInformation("Successfully got volunteer requests by relation user with id {id}", query.UserId);
        
        return result.ToList();
    }
}