using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Login;

public record LoginUserCommand(string Email, string Password) : ICommand;