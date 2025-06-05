using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.BanUser;

public record BanUserCommand(Guid UserId): ICommand;