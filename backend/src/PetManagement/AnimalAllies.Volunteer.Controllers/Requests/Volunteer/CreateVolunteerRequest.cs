using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record CreateVolunteerRequest(
    FullNameDto FullName,
    string Email,
    string Description,
    int WorkExperience,
    string PhoneNumber,
    IEnumerable<RequisiteDto> Requisites)
{
    public CreateVolunteerCommand ToCommand()
        => new(
            FullName,
            Email,
            Description,
            WorkExperience,
            PhoneNumber,
            Requisites);
}