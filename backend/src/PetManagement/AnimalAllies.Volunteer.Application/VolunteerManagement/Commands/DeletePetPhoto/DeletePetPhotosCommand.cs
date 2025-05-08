using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetPhoto;

public record DeletePetPhotosCommand(Guid VolunteerId, Guid PetId, IEnumerable<string> FilePaths) : ICommand;