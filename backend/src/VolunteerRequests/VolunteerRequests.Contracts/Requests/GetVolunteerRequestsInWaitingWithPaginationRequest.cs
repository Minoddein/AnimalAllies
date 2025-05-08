namespace VolunteerRequests.Contracts.Requests;

public record GetVolunteerRequestsInWaitingWithPaginationRequest(
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);