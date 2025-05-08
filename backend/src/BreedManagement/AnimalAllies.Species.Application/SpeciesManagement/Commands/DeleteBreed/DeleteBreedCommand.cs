using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteBreed;

public record DeleteBreedCommand(Guid SpeciesId, Guid BreedId) : ICommand;