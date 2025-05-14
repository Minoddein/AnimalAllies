using AnimalAllies.SharedKernel.Shared;

namespace VolunteerRequests.Domain.Events;

public record ApprovedVolunteerRequestDomainEvent(
    Guid UserId,
    Guid AdminId,
    string FirstName,
    string SecondName,
    string? Patronymic,
    string Description,
    string Email,
    string Phone,
    int WorkExperience): IDomainEvent;
