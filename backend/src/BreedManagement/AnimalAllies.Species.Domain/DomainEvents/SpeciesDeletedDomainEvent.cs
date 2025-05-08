using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Species.Domain.DomainEvents;

public record SpeciesDeletedDomainEvent(Guid SpeciesId) : IDomainEvent;