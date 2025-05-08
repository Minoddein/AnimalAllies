using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.ConfirmEmail;

public record ConfirmEmailCommand(Guid UserId, string Code) : ICommand;