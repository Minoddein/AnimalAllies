using Discussion.Domain.DomainEvents;
using MediatR;

namespace Discussion.Application.EventHandlers.CreatedDiscussion;

public class CreatedDiscussionEventHandler: INotificationHandler<CreatedDiscussionDomainEvent>
{
    public Task Handle(CreatedDiscussionDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}