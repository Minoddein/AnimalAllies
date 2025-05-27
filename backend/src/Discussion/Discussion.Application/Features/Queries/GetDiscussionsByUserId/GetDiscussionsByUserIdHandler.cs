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

public class GetDiscussionsByUserIdHandler : IQueryHandler<List<DiscussionDto>, GetDiscussionsByUserIdQuery>
{
    private readonly ILogger<GetDiscussionsByUserIdHandler> _logger;
    private readonly IValidator<GetDiscussionsByUserIdQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDiscussionsByUserIdHandler(
        ILogger<GetDiscussionsByUserIdHandler> logger,
        IValidator<GetDiscussionsByUserIdQuery> validator,
        [FromKeyedServices(Constraints.Context.Discussion)]
        ISqlConnectionFactory sqlConnectionFactory)
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

        parameters.Add("@UserId", query.UserId);

        var sql = new StringBuilder("""
                                    SELECT
                                        d.id,
                                        d.relation_id,
                                        -- Информация о последнем сообщении
                                        m.id as message_id,
                                        m.text,
                                        m.created_at,
                                        m.is_edited,
                                        m.user_id,
                                        -- Количество непрочитанных сообщений
                                        (
                                            SELECT COUNT(*) 
                                            FROM discussions.messages msg 
                                            WHERE msg.discussion_id = d.id 
                                            AND msg.user_id != @UserId 
                                            AND msg.is_read = false
                                        ) as unread_messages_count,
                                        -- Информация о пользователе (second_member)
                                        u.id as user_id,
                                        p.id as participant_id,
                                        p.user_id as participant_user_id,
                                        p.first_name as first_name,
                                        p.second_name as second_name,
                                        -- Информация об админе (first_member)
                                        admin_user.id as admin_id,
                                        admin_profile.user_id as admin_user_id,
                                        admin_profile.first_name as admin_first_name,
                                        admin_profile.second_name as admin_second_name
                                    FROM discussions.discussions d
                                    -- Соединение для последнего сообщения
                                    LEFT JOIN discussions.messages m ON m.id = d.last_message_id
                                    -- Соединения для пользователя (second_member)
                                    LEFT JOIN accounts.users u ON d.second_member = u.id
                                    LEFT JOIN accounts.participant_accounts p ON u.participant_account_id = p.id
                                    -- Соединения для админа (first_member)
                                    LEFT JOIN accounts.users admin_user ON d.first_member = admin_user.id
                                    LEFT JOIN accounts.admin_profiles admin_profile ON admin_user.id = admin_profile.user_id
                                    WHERE d.first_member = @UserId OR d.second_member = @UserId
                                    ORDER BY m.created_at DESC
                                    """);

        var result =
            await connection.QueryAsync
            <DiscussionDto, MessageDto, UserDto?, ParticipantAccountDto?, AdminProfileDto?, DiscussionDto>(
                sql.ToString(),
                (discussion, message, user, participant, admin) =>
                {
                    if (participant != null)
                    {
                        discussion.SecondMember = participant.ParticipantUserId;
                        discussion.SecondMemberName = participant.FirstName;
                        discussion.SecondMemberSurname = participant.SecondName;
                    }

                    if (admin != null)
                    {
                        discussion.FirstMember = admin.AdminUserId;
                        discussion.FirstMemberName = admin.AdminFirstName;
                        discussion.FirstMemberSurname = admin.AdminSecondName;
                    }
                    
                    discussion.LastMessage = message.Text;

                    return discussion;
                },
                splitOn: "message_id,user_id,participant_id, admin_id",
                param: parameters);

        _logger.LogInformation("Got discussions of user with id {id}", query.UserId);

        return result.ToList();
    }
}