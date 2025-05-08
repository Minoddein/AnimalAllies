using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Infrastructure.IdentityManagers;

public class AccountManager(
    AccountsDbContext accountsDbContext,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork) : IAccountManager
{
    public async Task CreateAdminAccount(AdminProfile adminProfile)
    {
        await accountsDbContext.AdminProfiles.AddAsync(adminProfile).ConfigureAwait(false);
        await unitOfWork.SaveChanges().ConfigureAwait(false);
    }

    public async Task<Result> CreateParticipantAccount(
        ParticipantAccount participantAccount, CancellationToken cancellationToken = default)
    {
        await accountsDbContext.ParticipantAccounts.AddAsync(participantAccount, cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    public async Task<Result> CreateVolunteerAccount(
        VolunteerAccount volunteerAccount, CancellationToken cancellationToken = default)
    {
        await accountsDbContext.VolunteerAccounts.AddAsync(volunteerAccount, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}