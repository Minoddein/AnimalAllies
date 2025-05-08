using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record CreateRequisitesRequest(IEnumerable<RequisiteDto> Requisites)
{
    public CreateRequisitesCommand ToCommand(Guid volunteerId)
        => new(volunteerId, Requisites);
}