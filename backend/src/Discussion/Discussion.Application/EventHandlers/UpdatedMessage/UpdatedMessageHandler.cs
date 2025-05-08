using Discussion.Domain.DomainEvents;
using MediatR;

namespace Discussion.Application.EventHandlers.UpdatedMessage;

public class UpdatedMessageHandler: INotificationHandler<UpdatedMessageDomainEvent>
{
    public Task Handle(UpdatedMessageDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}