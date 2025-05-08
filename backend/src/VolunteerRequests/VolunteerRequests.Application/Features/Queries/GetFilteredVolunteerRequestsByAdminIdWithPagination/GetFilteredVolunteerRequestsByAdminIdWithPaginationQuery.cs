using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByAdminIdWithPagination;

public record GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery(
    Guid AdminId,
    string? RequestStatus,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;