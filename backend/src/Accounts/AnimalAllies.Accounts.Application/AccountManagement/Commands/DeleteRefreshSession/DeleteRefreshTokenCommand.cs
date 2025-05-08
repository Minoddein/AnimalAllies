using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;

public record DeleteRefreshTokenCommand(Guid RefreshToken) : ICommand;