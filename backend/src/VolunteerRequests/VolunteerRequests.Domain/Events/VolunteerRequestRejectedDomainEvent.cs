using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record VolunteerRequestRejectedDomainEvent(
    Guid AdminId,
    Guid UserId, 
    string Email,
    string RejectionComment) : IDomainEvent;
