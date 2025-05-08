using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record ResentVolunteerRequestDomainEvent(Guid AdminId, Guid UserId) : IDomainEvent;