using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestoreVolunteer;

public record RestoreVolunteerCommand(Guid VolunteerId) : ICommand;