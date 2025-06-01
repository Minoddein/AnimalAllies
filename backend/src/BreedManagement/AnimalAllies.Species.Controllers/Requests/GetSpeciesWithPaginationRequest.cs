using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;

namespace AnimalAllies.Species.Presentation.Requests;

public record GetSpeciesWithPaginationRequest(
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize)
{
    public GetSpeciesWithPaginationQuery ToQuery()
        => new(SortBy, SortDirection, Page, PageSize);
}