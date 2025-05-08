using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Infrastructure.IdentityManagers;

public class PermissionManager(
    AccountsDbContext accountsDbContext,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork) : IPermissionManager
{
    public async Task<Result<List<string>>> GetPermissionsByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        User? user = await accountsDbContext.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Errors.General.NotFound();
        }

        List<string> permissions = [.. user.Roles.SelectMany(r => r.RolePermissions.Select(rp => rp.Permission.Code))];

        return permissions;
    }

    public async Task<Permission?> FindByCode(string code) =>
        await accountsDbContext.Permissions.FirstOrDefaultAsync(p => p.Code == code).ConfigureAwait(false);

    public async Task AddRangeIfExist(IEnumerable<string> permissions)
    {
        foreach (string permissionCode in permissions)
        {
            bool isPermissionExist = await accountsDbContext.Permissions
                .AnyAsync(p => p.Code == permissionCode).ConfigureAwait(false);

            if (isPermissionExist)
            {
                continue;
            }

            await accountsDbContext.Permissions.AddAsync(new Permission { Code = permissionCode })
                .ConfigureAwait(false);
        }

        await unitOfWork.SaveChanges().ConfigureAwait(false);
    }
}