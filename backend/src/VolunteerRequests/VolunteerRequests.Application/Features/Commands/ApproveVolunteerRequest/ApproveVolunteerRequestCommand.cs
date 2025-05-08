using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Commands.ApproveVolunteerRequest;

public record ApproveVolunteerRequestCommand(Guid AdminId, Guid VolunteerRequestId) : ICommand;