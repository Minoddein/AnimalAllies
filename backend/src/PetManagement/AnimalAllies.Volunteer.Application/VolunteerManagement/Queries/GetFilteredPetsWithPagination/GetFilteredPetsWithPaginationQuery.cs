using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetFilteredPetsWithPagination;

public record GetFilteredPetsWithPaginationQuery(
    Guid VolunteerId,
    Guid? BreedId,
    Guid? SpeciesId,
    string? Name,
    string? Color,
    string? Street,
    string? City,
    string? State,
    string? ZipCode,
    int? PositionFrom,
    int? PositionTo,
    int? WeightFrom,
    int? WeightTo,
    int? HeightFrom,
    int? HeightTo,
    bool? IsCastrated,
    bool? IsVaccinated,
    DateTime? BirthDateFrom,
    DateTime? BirthDateTo,
    string? HelpStatus,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;