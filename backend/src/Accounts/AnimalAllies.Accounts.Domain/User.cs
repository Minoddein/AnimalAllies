using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace AnimalAllies.Accounts.Domain;

public class User : IdentityUser<Guid>
{
    private List<Role> _roles = [];

    private List<SocialNetwork> _socialNetworks = [];

    private User()
    {
    }

    public string? Photo { get; set; }

    public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks;

    public IReadOnlyList<Role> Roles => _roles;

    public Guid? ParticipantAccountId { get; set; }

    public ParticipantAccount? ParticipantAccount { get; set; }

    public Guid? VolunteerAccountId { get; set; }

    public VolunteerAccount? VolunteerAccount { get; set; }

    public static User CreateAdmin(string userName, string email, Role role) =>
        new() { UserName = userName, Email = email, _roles = [role] };

    public Result AddSocialNetwork(IEnumerable<SocialNetwork> socialNetworks)
    {
        _socialNetworks = [.. socialNetworks];

        return Result.Success();
    }

    public static User CreateParticipant(
        string userName,
        string email,
        Role role) =>
        new() { UserName = userName, Email = email, _roles = [role] };

    public static User CreateVolunteer(
        string userName,
        string email,
        Role role) =>
        new() { UserName = userName, Email = email, _roles = [role] };
}