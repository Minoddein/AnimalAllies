using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPaginationBySearchTerm;

namespace AnimalAllies.Species.Presentation.Requests;

public record GetSpeciesWithPaginationBySearchTermRequest(
    string? SearchTerm,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize)
{
    public GetSpeciesWithPaginationBySearchTermQuery ToQuery()
        => new(SearchTerm, SortBy, SortDirection, Page, PageSize);
}