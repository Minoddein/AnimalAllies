using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CheckIfPetByBreedIdExist;

public record CheckIfPetByBreedIdExistQuery(Guid Id) : IQuery;