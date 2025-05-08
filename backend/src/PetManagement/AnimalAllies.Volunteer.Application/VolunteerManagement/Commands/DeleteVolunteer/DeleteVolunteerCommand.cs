using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;

public record DeleteVolunteerCommand(Guid Id) : ICommand;