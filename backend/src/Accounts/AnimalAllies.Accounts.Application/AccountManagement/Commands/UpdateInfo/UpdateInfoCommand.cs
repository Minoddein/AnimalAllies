using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateInfo;

public record UpdateInfoCommand(
    Guid UserId,
    string? FirstName,
    string? SecondName,
    string? Patronymic,
    string? Phone,
    int? Experience) : ICommand;
