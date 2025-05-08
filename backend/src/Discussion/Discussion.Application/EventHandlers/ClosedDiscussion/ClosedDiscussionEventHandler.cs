using Discussion.Domain.DomainEvents;
using MediatR;

namespace Discussion.Application.EventHandlers.ClosedDiscussion;

public class ClosedDiscussionEventHandler: INotificationHandler<ClosedDiscussionDomainEvent>
{
    public Task Handle(ClosedDiscussionDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}