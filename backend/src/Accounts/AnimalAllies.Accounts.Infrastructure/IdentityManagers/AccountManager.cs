using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Infrastructure.IdentityManagers;

public class AccountManager(
    AccountsDbContext accountsDbContext,
    [FromKeyedServices(Constraints.Context.Accounts)] IUnitOfWork unitOfWork) : IAccountManager
{
    public async Task CreateAdminAccount(AdminProfile adminProfile)
    {
        await accountsDbContext.AdminProfiles.AddAsync(adminProfile);
        await unitOfWork.SaveChanges();
    }
    
    public async Task<Result> CreateParticipantAccount(
        ParticipantAccount participantAccount, CancellationToken cancellationToken = default)
    {
        await accountsDbContext.ParticipantAccounts.AddAsync(participantAccount,cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CreateVolunteerAccount(
        VolunteerAccount volunteerAccount, CancellationToken cancellationToken = default)
    {
        await accountsDbContext.VolunteerAccounts.AddAsync(volunteerAccount,cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        
        return Result.Success();
    }

    public async Task<VolunteerAccount?> GetVolunteerAccount(Guid userId, CancellationToken cancellationToken = default)
    {
        var result =
            await accountsDbContext.VolunteerAccounts.FirstOrDefaultAsync(v =>
                v.UserId == userId, cancellationToken);
        
        return result;
    }
}