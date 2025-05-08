using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Species.Domain.DomainEvents;

public record SpeciesCreatedDomainEvent(Guid SpecieId) : IDomainEvent;