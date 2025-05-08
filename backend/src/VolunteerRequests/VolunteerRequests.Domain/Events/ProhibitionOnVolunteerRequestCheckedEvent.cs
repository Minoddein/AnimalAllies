using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record ProhibitionOnVolunteerRequestCheckedEvent(Guid UserId) : IDomainEvent;