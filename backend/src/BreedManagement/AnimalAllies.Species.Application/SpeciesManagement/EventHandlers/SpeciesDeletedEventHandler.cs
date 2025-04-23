using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class SpeciesDeletedEventHandler: INotificationHandler<SpeciesDeletedDomainEvent>
{
    public Task Handle(SpeciesDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}