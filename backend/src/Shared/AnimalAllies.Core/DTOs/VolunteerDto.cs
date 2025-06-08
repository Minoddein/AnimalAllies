using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs;

public class VolunteerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int WorkExperience { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AnimalsCount { get; set; }
    public Guid UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public RequisiteDto[] Requisites { get; set; } = [];
    public SocialNetworkDto[] SocialNetworks { get; set; } = [];
    public bool IsDeleted { get; set; }
}