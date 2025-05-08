using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByUserIdWithPagination;

public record GetFilteredVolunteerRequestsByUserIdWithPaginationQuery(
    Guid UserId,
    string? RequestStatus,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;