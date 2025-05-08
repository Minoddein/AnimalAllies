using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CheckIfPetByBreedIdExist;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CheckIfPetBySpeciesIdExist;
using AnimalAllies.Volunteer.Contracts;

namespace AnimalAllies.Volunteer.Presentation;

public class VolunteerContract(
    CheckIfPetBySpeciesIdExistHandler checkIfPetBySpeciesIdExistHandler,
    CheckIfPetByBreedIdExistHandler checkIfPetByBreedIdExistHandler) : IVolunteerContract
{
    private readonly CheckIfPetByBreedIdExistHandler _checkIfPetByBreedIdExistHandler = checkIfPetByBreedIdExistHandler;

    private readonly CheckIfPetBySpeciesIdExistHandler _checkIfPetBySpeciesIdExistHandler =
        checkIfPetBySpeciesIdExistHandler;

    public async Task<bool> CheckIfPetBySpeciesIdExist(
        Guid speciesId,
        CancellationToken cancellationToken = default)
    {
        CheckIfPetBySpeciesIdExistQuery query = new(speciesId);

        return (await _checkIfPetBySpeciesIdExistHandler.Handle(query, cancellationToken).ConfigureAwait(false)).Value;
    }

    public async Task<bool> CheckIfPetByBreedIdExist(
        Guid breedId,
        CancellationToken cancellationToken = default)
    {
        CheckIfPetByBreedIdExistQuery query = new(breedId);

        return (await _checkIfPetByBreedIdExistHandler.Handle(query, cancellationToken).ConfigureAwait(false)).Value;
    }
}