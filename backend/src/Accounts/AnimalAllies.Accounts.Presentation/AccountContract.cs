using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetBannedUserById;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetPermissionsByUserId;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;
using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts;
using AnimalAllies.Accounts.Domain;
using Microsoft.AspNetCore.Identity;

namespace AnimalAllies.Accounts.Presentation;

public class AccountContract(
    GetPermissionsByUserIdHandler getPermissionsByUserIdHandler,
    IsUserExistByIdHandler isUserExistByIdHandler,
    GetBannedUserByIdHandler getBannedUserByIdHandler,
    IAccountManager accountManager,
    UserManager<User> userManager) : IAccountContract
{
    private readonly IsUserExistByIdHandler _isUserExistByIdHandler = isUserExistByIdHandler;

    public async Task<bool> IsUserExistById(Guid userId, CancellationToken cancellationToken = default) =>
        (await _isUserExistByIdHandler.Handle(userId, cancellationToken).ConfigureAwait(false)).Value;
}