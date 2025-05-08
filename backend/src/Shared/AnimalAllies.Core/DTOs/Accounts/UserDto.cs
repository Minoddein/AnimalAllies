using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs.Accounts;

public class UserDto
{
    public Guid Id { get; set; }

    public required string UserName { get; set; }

    public string? Photo { get; set; }

    public SocialNetworkDto[] SocialNetworks { get; set; } = [];

    public RoleDto[] Roles { get; set; } = [];

    public VolunteerAccountDto? VolunteerAccount { get; set; }

    public Guid? VolunteerAccountId { get; set; }

    public ParticipantAccountDto? ParticipantAccount { get; set; }

    public Guid? ParticipantAccountId { get; set; }
}