using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetById;

public record GetPetByIdQuery(Guid PetId) : IQuery;