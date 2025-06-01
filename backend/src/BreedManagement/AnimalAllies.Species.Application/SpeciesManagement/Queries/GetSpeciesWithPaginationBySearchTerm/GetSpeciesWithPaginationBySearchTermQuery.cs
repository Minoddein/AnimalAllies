using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPaginationBySearchTerm;

public record GetSpeciesWithPaginationBySearchTermQuery(
    string? SearchTerm,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;
