using Discussion.Domain.DomainEvents;
using MediatR;

namespace Discussion.Application.EventHandlers.PostedMessage;

public class PostedMessageEventHandler: INotificationHandler<PostedMessageDomainEvent>
{
    public Task Handle(PostedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}