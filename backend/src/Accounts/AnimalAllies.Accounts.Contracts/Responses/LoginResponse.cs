namespace AnimalAllies.Accounts.Contracts.Responses;

public record LoginResponse(
    string AccessToken,
    Guid RefreshToken,
    Guid UserId,
    string UserName,
    string Email,
    string FirstName,
    string SecondName,
    string? Patronymic,
    string[] Roles,
    string[] Permissions);