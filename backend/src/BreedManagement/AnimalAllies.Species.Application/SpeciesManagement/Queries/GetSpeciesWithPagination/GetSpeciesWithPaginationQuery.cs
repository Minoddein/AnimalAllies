using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;

public record GetSpeciesWithPaginationQuery(
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;