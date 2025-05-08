namespace VolunteerRequests.Contracts.Requests;

public record UpdateRequest(
    Guid VolunteerRequestId,
    string FirstName,
    string SecondName,
    string? Patronymic,
    string Email,
    string PhoneNumber,
    int WorkExperience,
    string VolunteerDescription,
    IEnumerable<SocialNetworkRequest> SocialNetworks);