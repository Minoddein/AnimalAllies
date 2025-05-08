using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Commands.ResendVolunteerRequest;

public record ResendVolunteerRequestCommand(Guid UserId, Guid VolunteerRequestId) : ICommand;