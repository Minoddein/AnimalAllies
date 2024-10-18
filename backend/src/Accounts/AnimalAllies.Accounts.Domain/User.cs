﻿using System.Dynamic;
using AnimalAllies.SharedKernel.Shared;
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
    
    public static User CreateAdmin(string userName, string email, Role role)
    {
        return new User
        {
            UserName = userName,
            Email = email,
            _roles = [role]
        };
    }

    public Result AddSocialNetwork(IEnumerable<SocialNetwork> socialNetworks)
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

