using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Species.Domain.DomainEvents;

public record BreedCreatedDomainEvent(Guid SpeciesId, Guid BreedId) : IDomainEvent;