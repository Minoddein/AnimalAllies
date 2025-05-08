using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestorePet;

public record RestorePetCommand(Guid VolunteerId, Guid PetId) : ICommand;