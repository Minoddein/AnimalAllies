namespace Discussion.Contracts.Requests;

public record PostMessageRequest(Guid DiscussionId, string Text);