namespace Discussion.Contracts.Requests;

public record DeleteMessageRequest(Guid DiscussionId, Guid MessageId);