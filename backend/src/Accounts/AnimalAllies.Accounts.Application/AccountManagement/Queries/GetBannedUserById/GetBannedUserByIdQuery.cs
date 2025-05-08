using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetBannedUserById;

public record GetBannedUserByIdQuery(Guid UserId) : IQuery;