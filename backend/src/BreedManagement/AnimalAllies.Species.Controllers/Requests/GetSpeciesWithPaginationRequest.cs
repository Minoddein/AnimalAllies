using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;

namespace AnimalAllies.Species.Presentation.Requests;

public record GetSpeciesWithPaginationRequest(
    string? SearchTerm,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize)
{
    public GetSpeciesWithPaginationQuery ToQuery()
        => new(SearchTerm, SortBy, SortDirection, Page, PageSize);
}