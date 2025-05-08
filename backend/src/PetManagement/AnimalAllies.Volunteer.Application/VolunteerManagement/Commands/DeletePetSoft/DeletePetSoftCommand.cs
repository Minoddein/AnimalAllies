using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetSoft;

public record DeletePetSoftCommand(Guid VolunteerId, Guid PetId) : ICommand;