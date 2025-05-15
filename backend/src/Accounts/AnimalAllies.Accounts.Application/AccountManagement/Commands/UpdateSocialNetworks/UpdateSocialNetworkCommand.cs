using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateSocialNetworks;

public record UpdateSocialNetworkCommand(Guid UserId, IEnumerable<SocialNetworkDto> SocialNetworkDtos) : ICommand;