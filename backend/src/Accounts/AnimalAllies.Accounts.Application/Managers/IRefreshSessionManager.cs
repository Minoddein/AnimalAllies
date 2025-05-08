using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Accounts.Application.Managers;

public interface IRefreshSessionManager
{
    Task Delete(RefreshSession refreshSession, CancellationToken cancellationToken = default);

    Task<Result<RefreshSession>> GetByRefreshToken(Guid refreshToken, CancellationToken cancellationToken = default);
}