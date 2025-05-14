﻿
namespace VolunteerRequests.Contracts.Messaging;

public record ApprovedVolunteerRequestEvent(
    Guid UserId,
    string FirstName,
    string SecondName,
    string? Patronymic,
    int WorkExperience,
    string Description,
    string Email,
    string Phone);
