using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;

public record GetVolunteerRequestsInWaitingWithPaginationQuery(
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;