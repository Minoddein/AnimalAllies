namespace AnimalAllies.Volunteer.Contracts;

public interface IVolunteerContract
{
    Task<bool> CheckIfPetBySpeciesIdExist(Guid speciesId, CancellationToken cancellationToken = default);

    Task<bool> CheckIfPetByBreedIdExist(Guid breedId, CancellationToken cancellationToken = default);
}