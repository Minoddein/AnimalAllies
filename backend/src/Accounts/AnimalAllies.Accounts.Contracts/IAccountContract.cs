namespace AnimalAllies.Accounts.Contracts;

public interface IAccountContract
{
    Task<bool> IsUserExistById(Guid userId, CancellationToken cancellationToken = default);

    Task ApproveVolunteerRequest(
        Guid userId,
        string firstName,
        string secondName,
        string? patronymic,
        int workExperience,
        string description,
        string email,
        string phone,
        CancellationToken cancellationToken = default);
}