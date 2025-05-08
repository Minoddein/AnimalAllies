using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Queries.GetDiscussionByRelationId;

public record GetDiscussionByRelationIdQuery(
    Guid RelationId,
    int PageSize) : IQuery;