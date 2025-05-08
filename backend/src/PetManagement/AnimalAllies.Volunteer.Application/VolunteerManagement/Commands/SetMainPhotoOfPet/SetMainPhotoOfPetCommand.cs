using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.SetMainPhotoOfPet;

public record SetMainPhotoOfPetCommand(Guid VolunteerId, Guid PetId, string Path) : ICommand;