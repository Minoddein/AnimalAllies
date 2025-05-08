using Discussion.Domain.DomainEvents;
using MediatR;

namespace Discussion.Application.EventHandlers.DeletedMessage;

public class DeletedMessageEventHandler: INotificationHandler<DeletedMessageDomainEvent>
{
    public Task Handle(DeletedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}