namespace Discussion.Contracts.Requests;

public record CreateDiscussionRequest(Guid SecondMemberId, Guid RelationId);
