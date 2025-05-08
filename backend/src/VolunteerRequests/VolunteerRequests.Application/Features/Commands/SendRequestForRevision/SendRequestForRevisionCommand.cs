using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Commands.SendRequestForRevision;

public record SendRequestForRevisionCommand(Guid AdminId, Guid VolunteerRequestId, string RejectionComment) : ICommand;