using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetPermissionsByUserId;

public record GetPermissionsByUserIdQuery(Guid UserId) : IQuery;