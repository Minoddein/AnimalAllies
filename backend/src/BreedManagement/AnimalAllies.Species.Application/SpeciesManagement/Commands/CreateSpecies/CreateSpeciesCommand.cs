using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateSpecies;

public record CreateSpeciesCommand(string Name) : ICommand;