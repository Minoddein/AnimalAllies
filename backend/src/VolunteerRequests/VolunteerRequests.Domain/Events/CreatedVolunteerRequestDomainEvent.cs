using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record CreatedVolunteerRequestDomainEvent(Guid VolunteerRequestId, Guid UserId, string Email) : IDomainEvent;