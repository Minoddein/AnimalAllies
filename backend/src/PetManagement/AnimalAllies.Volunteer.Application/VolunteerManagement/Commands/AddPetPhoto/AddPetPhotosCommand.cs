using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.FileService;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;

public record AddPetPhotosCommand(Guid VolunteerId, Guid PetId, IEnumerable<UploadFileDto> Photos) : ICommand;