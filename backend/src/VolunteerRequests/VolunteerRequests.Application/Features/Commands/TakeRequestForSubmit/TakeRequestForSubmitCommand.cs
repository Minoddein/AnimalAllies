using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Commands.TakeRequestForSubmit;

public record TakeRequestForSubmitCommand(Guid AdminId, Guid VolunteerRequestId) : ICommand;