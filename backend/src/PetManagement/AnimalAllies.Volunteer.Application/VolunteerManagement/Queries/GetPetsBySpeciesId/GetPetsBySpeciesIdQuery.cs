using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsBySpeciesId;

public record GetPetsBySpeciesIdQuery(Guid SpeciesId, int Page, int PageSize) : IQuery;