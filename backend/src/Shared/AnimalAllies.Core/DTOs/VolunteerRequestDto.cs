using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs;

public class VolunteerRequestDto
{
    public Guid Id { get; set; }

    public string RequestStatus { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string SecondName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string VolunteerDescription { get; set; } = string.Empty;

    public int WorkExperience { get; set; }

    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public Guid DiscussionId { get; set; }

    public string RejectionComment { get; set; } = string.Empty;

    public SocialNetworkDto[] SocialNetworks { get; set; } = [];
}