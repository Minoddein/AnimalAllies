using AnimalAllies.Core.DTOs.FileService;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record AddPetPhotosRequest(IEnumerable<UploadFileDto> Files)
{
    public AddPetPhotosCommand ToCommand(Guid volunteerId, Guid petId)
        => new(volunteerId, petId, Files);
}