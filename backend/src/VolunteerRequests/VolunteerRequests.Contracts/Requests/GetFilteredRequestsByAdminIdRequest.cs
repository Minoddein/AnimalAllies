namespace VolunteerRequests.Contracts.Requests;

public record GetFilteredRequestsByAdminIdRequest(
    string? RequestStatus,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);