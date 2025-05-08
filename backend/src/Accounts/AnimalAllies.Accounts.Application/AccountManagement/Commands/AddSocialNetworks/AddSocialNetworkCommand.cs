using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddSocialNetworks;

public record AddSocialNetworkCommand(Guid UserId, IEnumerable<SocialNetworkDto> SocialNetworkDtos) : ICommand;