using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Queries.GetDiscussionByRelationId;

public class GetDiscussionByRelationIdHandler : IQueryHandler<DiscussionDto?, GetDiscussionByRelationIdQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetDiscussionByRelationIdHandler> _logger;
    private readonly HybridCache _hybridCache;

    public GetDiscussionByRelationIdHandler(
        [FromKeyedServices(Constraints.Context.Discussion)]
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetDiscussionByRelationIdHandler> logger,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _hybridCache = hybridCache;
    }

    public async Task<Result<DiscussionDto?>> Handle(
        GetDiscussionByRelationIdQuery query, CancellationToken cancellationToken = default)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(1),
            LocalCacheExpiration = TimeSpan.FromSeconds(15)
        };

        var cachedDiscussion = await _hybridCache.GetOrCreateAsync(
            key: $"{TagsConstants.DISCUSSIONS}_{query.RelationId}",
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
                parameters.Add("@RelationId", query.RelationId);

                var sql = new StringBuilder("""
                                            select
                                                d.id,
                                                relation_id,
                                                m.id as message_id,
                                                m.text,
                                                m.created_at,
                                                m.is_edited,
                                                m.is_read,
                                                m.user_id,
                                                u.id as user_id,
                                                -- Информация о пользователе (second_member)
                                                p.id as participant_id,
                                                p.user_id as participant_user_id,
                                                p.first_name as first_name,
                                                p.second_name as second_name,
                                                -- Информация об админе (first_member)
                                                admin_user.id as admin_id,
                                                admin_profile.user_id as admin_user_id,
                                                admin_profile.first_name as admin_first_name,
                                                admin_profile.second_name as admin_second_name
                                            from discussions.discussions d
                                                     left join discussions.messages m on m.discussion_id = d.id
                                                     -- Соединения для пользователя (second_member)
                                                     LEFT JOIN accounts.users u ON d.second_member = u.id
                                                     LEFT JOIN accounts.participant_accounts p ON u.participant_account_id = p.id
                                                     -- Соединения для админа (first_member)
                                                     LEFT JOIN accounts.users admin_user ON d.first_member = admin_user.id
                                                     LEFT JOIN accounts.admin_profiles admin_profile ON admin_user.id = admin_profile.user_id
                                            where relation_id = @RelationId
                                            order by m.created_at
                                            """);

                var discussion =
                    await connection
                        .QueryAsync
                            <DiscussionDto, MessageDto, UserDto, ParticipantAccountDto?, AdminProfileDto?, DiscussionDto>(
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
                                
                                discussion.Messages = [message];

                                return discussion;
                            },
                            splitOn: "message_id,user_id,participant_id, admin_id",
                            param: parameters);

                var discussionDtos = discussion as DiscussionDto[] ?? discussion.ToArray();
                if (!discussionDtos.Any())
                    return null;

                var messages = discussionDtos.SelectMany(d => d.Messages).ToList();

                var discussionResult = discussionDtos.First();

                discussionResult.Messages = messages.ToArray();

                return discussionResult;
            },
            options: options,
            tags: [new string(TagsConstants.DISCUSSIONS + "_" + query.RelationId)],
            cancellationToken: cancellationToken);

        if (cachedDiscussion is null)
            return null;

        _logger.LogInformation("Got message from discussion with relation id {id}", query.RelationId);

        return cachedDiscussion;
    }
}