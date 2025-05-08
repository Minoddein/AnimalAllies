using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record SentRequestForRevisionDomainEvent(
    Guid VolunteerRequestId,
    Guid AdminId,
    Guid UserId,
    string Email) : IDomainEvent;