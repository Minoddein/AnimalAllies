using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Queries.GetDiscussionsByUserId;

public record GetDiscussionsByUserIdQuery(Guid UserId) : IQuery;
