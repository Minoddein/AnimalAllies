namespace Discussion.Contracts;

public interface IDiscussionContract
{
    Task<Guid> CreateDiscussionHandler(
        Guid firstMember,
        Guid secondMember,
        Guid relationId,
        CancellationToken cancellationToken = default);
}