using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsByBreedId;

public record GetPetsByBreedIdQuery(Guid BreedId, int Page, int PageSize) : IQuery;