using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.FileService;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;

public record AddPetPhotosCommand(Guid VolunteerId, Guid PetId, IEnumerable<UploadFileDto> Photos) : ICommand;
