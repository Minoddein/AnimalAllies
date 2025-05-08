namespace AnimalAllies.Core.DTOs.ValueObjects;

public record UpdateVolunteerMainInfoDto(
    FullNameDto FullName,
    string Email,
    string Description,
    int WorkExperience,
    string PhoneNumber);