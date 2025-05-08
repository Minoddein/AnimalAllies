using System.Data;
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

public class GetDiscussionByRelationIdHandler(
    [FromKeyedServices(Constraints.Context.Discussion)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetDiscussionByRelationIdHandler> logger,
    HybridCache hybridCache) : IQueryHandler<List<MessageDto>, GetDiscussionByRelationIdQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetDiscussionByRelationIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<Result<List<MessageDto>>> Handle(
        GetDiscussionByRelationIdQuery query, CancellationToken cancellationToken = default)
    {
        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(1), LocalCacheExpiration = TimeSpan.FromSeconds(15)
        };

        List<MessageDto> cacheMessages = await _hybridCache.GetOrCreateAsync(
            $"{TagsConstants.DISCUSSIONS}_{query.RelationId}",
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();
                parameters.Add("@RelationId", query.RelationId);
                parameters.Add("@PageSize", query.PageSize);

                StringBuilder sql = new("""
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
                                            p.first_name
                                        from discussions.discussions d
                                                 left join discussions.messages m on m.discussion_id = d.id
                                                 left join accounts.users u on m.user_id = u.id
                                                 left join accounts.participant_accounts p on u.participant_account_id = p.id
                                        where relation_id = @RelationId
                                        order by m.id
                                        limit @PageSize
                                        """);

                IEnumerable<DiscussionDto> discussion =
                    await connection
                        .QueryAsync<DiscussionDto, MessageDto, UserDto, ParticipantAccountDto, DiscussionDto>(
                            sql.ToString(),
                            (discussion, message, user, participant) =>
                            {
                                message.FirstName = participant.FirstName;
                                discussion.Messages = [message];

                                return discussion;
                            },
                            splitOn: "message_id,user_id,participant_id",
                            param: parameters).ConfigureAwait(false);

                DiscussionDto[] discussionDtos = discussion as DiscussionDto[] ?? [.. discussion];
                if (!discussionDtos.Any())
                {
                    return [];
                }

                List<MessageDto> messages = [.. discussionDtos.SelectMany(d => d.Messages)];

                return messages;
            },
            options,
            [new string(TagsConstants.DISCUSSIONS + "_" + query.RelationId)],
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Got message from discussion with relation id {id}", query.RelationId);

        return cacheMessages;
    }
}