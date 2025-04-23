namespace AnimalAllies.Species.Contracts.Events;

public record BreedCreatedIntegrationEvent(Guid SpeciesId, Guid BreedId);
