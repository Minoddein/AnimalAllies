namespace VolunteerRequests.Contracts.Requests;

public record CreateVolunteerRequestRequest(
    string FirstName,
    string SecondName,
    string? Patronymic,
    string Email,
    string PhoneNumber,
    int WorkExperience,
    string VolunteerDescription,
    IEnumerable<SocialNetworkRequest> SocialNetworks);

public class SocialNetworkRequest
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}