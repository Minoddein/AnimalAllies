using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;

public record IsUserExistByIdQuery(Guid UserId) : IQuery;