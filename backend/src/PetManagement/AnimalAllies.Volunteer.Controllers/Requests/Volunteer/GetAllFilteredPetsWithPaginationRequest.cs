using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetAllFilteredPetsWithPagination;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetFilteredPetsWithPaginationByVolunteerId;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record GetAllFilteredPetsWithPaginationRequest(
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
    int PageSize)
{
    public GetAllFilteredPetsWithPaginationQuery ToQuery()
        => new(
            BreedId,
            SpeciesId,
            Name,
            Color,
            Street,
            City,
            State,
            ZipCode,
            PositionFrom,
            PositionTo,
            WeightFrom,
            WeightTo,
            HeightFrom,
            HeightTo,
            IsCastrated,
            IsVaccinated,
            BirthDateFrom,
            BirthDateTo,
            HelpStatus,
            SortBy,
            SortDirection,
            Page,
            PageSize);
}