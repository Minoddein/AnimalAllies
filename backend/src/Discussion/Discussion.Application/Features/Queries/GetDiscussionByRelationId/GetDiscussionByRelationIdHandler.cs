using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Queries.GetDiscussionByRelationId;

public class GetDiscussionByRelationIdHandler: IQueryHandler<List<MessageDto>, GetDiscussionByRelationIdQuery>
{
    private const string REDIS_KEY = "discussions_";
    
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetDiscussionByRelationIdHandler> _logger;
    private readonly HybridCache _hybridCache;

    public GetDiscussionByRelationIdHandler(
        [FromKeyedServices(Constraints.Context.Discussion)]ISqlConnectionFactory sqlConnectionFactory, 
        ILogger<GetDiscussionByRelationIdHandler> logger,
        HybridCache hybridCache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
        _hybridCache = hybridCache;
    }

    public async Task<Result<List<MessageDto>>> Handle(
        GetDiscussionByRelationIdQuery query, CancellationToken cancellationToken = default)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(1)
        };

        var cacheMessages = await _hybridCache.GetOrCreateAsync(
            key: REDIS_KEY + query.RelationId,
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
                parameters.Add("@RelationId",query.RelationId);
                parameters.Add("@PageSize",query.PageSize);
        
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
                                                p.first_name
                                            from discussions.discussions d
                                                     left join discussions.messages m on m.discussion_id = d.id
                                                     left join accounts.users u on m.user_id = u.id
                                                     left join accounts.participant_accounts p on u.participant_account_id = p.id
                                            where relation_id = @RelationId
                                            order by m.id
                                            limit @PageSize
                                            """);
                
                var discussion = 
                    await connection.QueryAsync<DiscussionDto,MessageDto, UserDto, ParticipantAccountDto, DiscussionDto>(
                        sql.ToString(),
                        (discussion, message, user, participant) =>
                        {
                            message.FirstName = participant.FirstName;
                            discussion.Messages = [message];
                    
                            return discussion;
                        },
                        splitOn:"message_id,user_id,participant_id",
                        param: parameters);

                if (!discussion.Any())
                    return [];

                var messages = discussion.SelectMany(d => d.Messages).ToList();

                return messages;
            },
            options: options,
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Got message from discussion with relation id {id}", query.RelationId);

        return cacheMessages;
    }
}