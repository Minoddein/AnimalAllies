using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateSkills;

public record UpdateSkillsCommand(Guid VolunteerId, IEnumerable<SkillDto> Skills) : ICommand;
