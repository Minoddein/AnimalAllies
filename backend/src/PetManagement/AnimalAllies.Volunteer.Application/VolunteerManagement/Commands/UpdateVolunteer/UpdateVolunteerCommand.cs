using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;

public record UpdateVolunteerCommand(Guid Id, UpdateVolunteerMainInfoDto Dto) : ICommand;