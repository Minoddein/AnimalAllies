using AnimalAllies.Species.Domain.DomainEvents;
using MediatR;

namespace AnimalAllies.Species.Application.SpeciesManagement.EventHandlers;

public class SpeciesCreatedEventHandler: INotificationHandler<SpeciesCreatedDomainEvent>
{
    public Task Handle(SpeciesCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}