using AnimalAllies.Core.DTOs;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateSkills;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record UpdateSkillsRequest(IEnumerable<SkillDto> SkillsDtos)
{
    public UpdateSkillsCommand ToCommand(Guid volunteerId)
        => new(volunteerId, SkillsDtos);
}