using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.MovePetPosition;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record MovePetPositionRequest(PositionDto Position)
{
    public MovePetPositionCommand ToCommand(Guid volunteerId, Guid petId)
        => new(volunteerId, petId, Position);
}