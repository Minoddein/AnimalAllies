using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Infrastructure.IdentityManagers;

public class RefreshSessionManager(
    AccountsDbContext accountsDbContext,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork) : IRefreshSessionManager
{
    public async Task<Result<RefreshSession>> GetByRefreshToken(
        Guid refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshSession? refreshSession = await accountsDbContext.RefreshSessions
            .Include(r => r.User)
            .ThenInclude(u => u.ParticipantAccount)
            .Include(r => r.User)
            .ThenInclude(u => u.Roles)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken).ConfigureAwait(false);

        if (refreshSession is null)
        {
            return Errors.General.NotFound();
        }

        return refreshSession;
    }

    public async Task Delete(RefreshSession refreshSession, CancellationToken cancellationToken = default)
    {
        accountsDbContext.RefreshSessions.Remove(refreshSession);
        await unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);
    }
}