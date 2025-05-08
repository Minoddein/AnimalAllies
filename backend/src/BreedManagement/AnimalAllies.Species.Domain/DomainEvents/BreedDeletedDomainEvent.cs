using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Species.Domain.DomainEvents;

public record BreedDeletedDomainEvent(Guid SpeciesId, Guid BreedId) : IDomainEvent;