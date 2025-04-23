namespace AnimalAllies.Species.Contracts.Events;

public record BreedDeletedIntegrationEvent(Guid SpeciesId, Guid BreedId);
