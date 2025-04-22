namespace Discussion.Contracts;

public interface IDiscussionContract
{
    public Task<Guid> CreateDiscussionHandler(
        Guid firstMember,
        Guid secondMember,
        Guid relationId,
        CancellationToken cancellationToken = default);
}