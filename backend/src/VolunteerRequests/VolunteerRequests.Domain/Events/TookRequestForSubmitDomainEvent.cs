using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record TookRequestForSubmitDomainEvent(
    Guid AdminId,
    Guid UserId,
    string Email) : IDomainEvent;