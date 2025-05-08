using AnimalAllies.Core.DTOs;

namespace AnimalAllies.Volunteer.Application.Database;

public interface IReadDbContext
{
    IQueryable<VolunteerDto> Volunteers { get; }

    IQueryable<PetDto> Pets { get; }
}