using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUsersWithPagination;

public record GetUsersWithPaginationQuery(int Page, int PageSize) : IQuery;