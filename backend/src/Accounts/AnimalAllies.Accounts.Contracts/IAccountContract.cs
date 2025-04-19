namespace AnimalAllies.Accounts.Contracts;

public interface IAccountContract
{
    Task<bool> IsUserExistById(Guid userId, CancellationToken cancellationToken = default);
}