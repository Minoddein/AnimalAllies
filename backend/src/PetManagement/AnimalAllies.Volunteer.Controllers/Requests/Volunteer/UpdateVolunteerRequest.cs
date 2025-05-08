using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record UpdateVolunteerRequest(UpdateVolunteerMainInfoDto Dto)
{
    public UpdateVolunteerCommand ToCommand(Guid volunteerId)
        => new(volunteerId, Dto);
}