using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record UpdatedVolunteerRequestDomainEvent(Guid AdminId, Guid UserId) : IDomainEvent;