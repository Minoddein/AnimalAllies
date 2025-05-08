using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteersWithPagination;

public record GetFilteredVolunteersWithPaginationQuery(
    string? FirstName,
    string? SecondName,
    string? Patronymic,
    int? WorkExperienceFrom,
    int? WorkExperienceTo,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;