using System.Dynamic;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace AnimalAllies.Accounts.Domain;

public class User:IdentityUser<Guid>
{
    private User()
    {
        
    }
    
    public string? Photo { get; set; }

    private List<SocialNetwork> _socialNetworks = [];
    public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks;
    
    private List<Role> _roles = [];
    public IReadOnlyList<Role> Roles => _roles;
    
    public Guid? ParticipantAccountId { get; set; }
    public ParticipantAccount? ParticipantAccount { get; set; }
    
    public Guid? VolunteerAccountId { get; set; }
    public VolunteerAccount? VolunteerAccount { get; set; }
    
    public void AddRole(Role role) => _roles.Add(role);
    
    public static User CreateAdmin(string userName, string email, Role role)
    {
        return new User
        {
            UserName = userName,
            Email = email,
            _roles = [role]
        };
    }

    public Result UpdateSocialNetwork(IEnumerable<SocialNetwork> socialNetworks)
    {
        _socialNetworks = socialNetworks.ToList();

        return Result.Success();
    }
    
    public static User CreateParticipant(
        string userName,
        string email,
        Role role)
    {
        return new User
        {
            UserName = userName,
            Email = email,
            _roles = [role]
        };
    }
    
}

