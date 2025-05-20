using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Infrastructure.IdentityManagers;

public class RefreshSessionManager(
    AccountsDbContext accountsDbContext,
    [FromKeyedServices(Constraints.Context.Accounts)] IUnitOfWork unitOfWork,
    ILogger<RefreshSessionManager> logger) : IRefreshSessionManager
{

    public async Task<Result<RefreshSession>> GetByRefreshToken(
        Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshSession = await accountsDbContext.RefreshSessions
            .Include(r => r.User)
                .ThenInclude(u => u.ParticipantAccount)
            .Include(r => r.User)
                .ThenInclude(u => u.Roles)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(r => r.User)
                .ThenInclude(u => u.VolunteerAccount)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken);

        if (refreshSession is null)
            return Errors.General.NotFound();

        return refreshSession;
    }
    
    public async Task Delete(RefreshSession refreshSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingSession = await accountsDbContext.RefreshSessions
                .FirstOrDefaultAsync(r => r.RefreshToken == refreshSession.RefreshToken, cancellationToken);
            
            if (existingSession != null)
            {
                accountsDbContext.RefreshSessions.Remove(existingSession);
                await unitOfWork.SaveChanges(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError("RefreshSession already deleted or modified: " + ex.Message);
        }
    }
}