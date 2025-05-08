using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;

public record CreateRequisitesCommand(Guid Id, IEnumerable<RequisiteDto> RequisiteDtos) : ICommand;