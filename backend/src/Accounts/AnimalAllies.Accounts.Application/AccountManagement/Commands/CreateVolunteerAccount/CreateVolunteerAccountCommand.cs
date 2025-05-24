using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.CreateVolunteerAccount;

public record CreateVolunteerAccountCommand(Guid UserId,
    string FirstName,
    string SecondName,
    string? Patronymic,
    int WorkExperience,
    string Description,
    string Email,
    string Phone) : ICommand;