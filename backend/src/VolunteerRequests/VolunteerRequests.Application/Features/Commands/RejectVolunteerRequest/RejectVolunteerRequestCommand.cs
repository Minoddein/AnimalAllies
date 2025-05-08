using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Commands.RejectVolunteerRequest;

public record RejectVolunteerRequestCommand(Guid AdminId, Guid VolunteerRequestId, string RejectionComment) : ICommand;