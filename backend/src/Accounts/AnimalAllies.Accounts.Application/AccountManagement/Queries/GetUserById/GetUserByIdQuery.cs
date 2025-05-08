using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IQuery;