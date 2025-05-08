using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;

public record RefreshTokensCommand(Guid RefreshToken) : ICommand;