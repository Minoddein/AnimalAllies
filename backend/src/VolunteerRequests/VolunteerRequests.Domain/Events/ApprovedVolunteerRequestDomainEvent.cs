using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record ApprovedVolunteerRequestDomainEvent(
    Guid UserId,
    Guid AdminId,
    string FirstName,
    string SecondName,
    string? Patronymic,
    string Email,
    int WorkExperience) : IDomainEvent;