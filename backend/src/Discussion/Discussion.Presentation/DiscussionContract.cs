using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Application.Features.Commands.CreateDiscussion;
using Discussion.Contracts;

namespace Discussion.Presentation;

public class DiscussionContract(CreateDiscussionHandler createDiscussionHandler) : IDiscussionContract
{
    private readonly CreateDiscussionHandler _createDiscussionHandler = createDiscussionHandler;

    public async Task<Guid> CreateDiscussionHandler(
        Guid firstMember,
        Guid secondMember,
        Guid relationId,
        CancellationToken cancellationToken = default)
    {
        Result<DiscussionId> result =
            await _createDiscussionHandler.Handle(firstMember, secondMember, relationId, cancellationToken)
                .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Guid.Empty;
        }

        return result.Value.Id;
    }
}