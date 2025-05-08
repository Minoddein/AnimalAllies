namespace Discussion.Contracts.Requests;

public record GetMessagesByRelationIdRequest(
    Guid RelationId,
    int PageSize);