namespace AnimalAllies.Core.DTOs.ValueObjects;

public record UpdateVolunteerMainInfoDto(
    FullNameDto FullName,
    int WorkExperience,
    string PhoneNumber);