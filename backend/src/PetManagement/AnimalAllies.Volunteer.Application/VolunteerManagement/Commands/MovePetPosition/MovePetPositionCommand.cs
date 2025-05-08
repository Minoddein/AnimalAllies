using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.MovePetPosition;

public record MovePetPositionCommand(Guid VolunteerId, Guid PetId, PositionDto Position) : ICommand;