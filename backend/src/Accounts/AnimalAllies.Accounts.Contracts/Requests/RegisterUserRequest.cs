namespace AnimalAllies.Accounts.Contracts.Requests;

public record FullNameDto(string FirstName, string SecondName, string Patronymic);

public record RegisterUserRequest(
    string Email,
    string UserName,
    FullNameDto FullNameDto,
    string Password);
