using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Queries.GetDiscussionsByUserId;

public class GetDiscussionsByUserIdHandler: IQueryHandler<List<DiscussionDto>,GetDiscussionsByUserIdQuery>
{
    private readonly ILogger<GetDiscussionsByUserIdHandler> _logger;
    private readonly IValidator<GetDiscussionsByUserIdQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDiscussionsByUserIdHandler(
        ILogger<GetDiscussionsByUserIdHandler> logger, 
        IValidator<GetDiscussionsByUserIdQuery> validator,
        [FromKeyedServices(Constraints.Context.Discussion)]ISqlConnectionFactory sqlConnectionFactory)
    {
        _logger = logger;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<DiscussionDto>>> Handle(
        GetDiscussionsByUserIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }
        
        var connection = _sqlConnectionFactory.Create();
        
        var parameters = new DynamicParameters();
        
        parameters.Add("@UserId",query.UserId);
        
        var sql = new StringBuilder("""
                                    select
                                        d.id,
                                        relation_id,
                                        m.id as message_id,
                                        m.text,
                                        m.created_at,
                                        m.is_edited,
                                        m.user_id,
                                        u.id as user_id,
                                        p.id as participant_id,
                                        p.first_name,
                                        p.second_name
                                    from discussions.discussions d
                                             left join accounts.users u on d.second_member = u.id
                                             left join accounts.participant_accounts p on u.participant_account_id = p.id
                                             left join discussions.messages m on m.id = d.last_message_id
                                    where d.first_member = @UserId or d.second_member = @UserId
                                    order by m.id
                                    """);
        
        var result = await connection.QueryAsync<DiscussionDto,MessageDto, UserDto?, ParticipantAccountDto?, DiscussionDto>(
            sql.ToString(),
            (discussion, message, user, participant) =>
            {
                if (participant != null)
                {
                    discussion.FirstMember = participant.ParticipantId;
                    discussion.SecondMemberName = participant.FirstName;
                    discussion.SecondMemberSurname = participant.SecondName;
                }
                
                discussion.LastMessage = message.Text;
                    
                return discussion;
            },
            splitOn:"message_id,user_id,participant_id",
            param: parameters);
        
        _logger.LogInformation("Got discussions of user with id {id}", query.UserId);

        return result.ToList();
    }
}