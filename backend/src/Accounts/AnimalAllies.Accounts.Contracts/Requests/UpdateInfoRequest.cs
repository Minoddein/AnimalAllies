namespace AnimalAllies.Accounts.Contracts.Requests;

public record UpdateInfoRequest(
    string? FirstName,
    string? SecondName,
    string? Patronymic, 
    string? Phone,
    int? Experience);