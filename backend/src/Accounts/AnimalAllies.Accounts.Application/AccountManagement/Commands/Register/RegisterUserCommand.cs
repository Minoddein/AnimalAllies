using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Register;

public record RegisterUserCommand(
    string Email,
    string UserName,
    FullNameDto FullNameDto,
    string Password) : ICommand;