using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Contracts;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;

public class AddPetHandler : ICommandHandler<AddPetCommand, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly ISpeciesContracts _speciesContracts;
    private readonly ILogger<AddPetHandler> _logger;
    private readonly IValidator<AddPetCommand> _validator;

    public AddPetHandler(
        IVolunteerRepository volunteerRepository,
        ILogger<AddPetHandler> logger,
        IDateTimeProvider dateTimeProvider,
        IValidator<AddPetCommand> validator,
        ISpeciesContracts speciesContracts)
    {
        _volunteerRepository = volunteerRepository;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
        _speciesContracts = speciesContracts;
    }


    public async Task<Result<Guid>> Handle(
        AddPetCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        var volunteerResult = await _volunteerRepository.GetById(
            VolunteerId.Create(command.VolunteerId), cancellationToken);

        if (volunteerResult.IsFailure)
            return volunteerResult.Errors;

        var pet = await ArrangePet(command, cancellationToken);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        volunteerResult.Value.AddPet(pet.Value);

        await _volunteerRepository.Save(volunteerResult.Value, cancellationToken);

        _logger.LogInformation("added pet with id {petId} to volunteer with id {volunteerId}", pet.Value.Id.Id,
            volunteerResult.Value.Id.Id);

        return pet.Value.Id.Id;
    }

    private async Task<Result<Pet>> ArrangePet(AddPetCommand command, CancellationToken cancellationToken)
    {
        var petId = PetId.NewGuid();

        var name = Name.Create(command.Name).Value;

        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;

        var helpStatus = HelpStatus.Create(command.HelpStatus).Value;

        var petPhysicCharacteristics = PetPhysicCharacteristics.Create(
            command.PetPhysicCharacteristics.Color,
            command.PetPhysicCharacteristics.HealthInformation,
            command.PetPhysicCharacteristics.Weight,
            command.PetPhysicCharacteristics.Height).Value;

        var petDetails = PetDetails.Create(
            command.PetDetails.Description,
            DateOnly.FromDateTime(command.PetDetails.BirthDate),
            _dateTimeProvider.UtcNow).Value;

        var address = Address.Create(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.ZipCode).Value;

        var species = await _speciesContracts.GetSpecies(cancellationToken);
        if (!species.Any())
            return Error.NotFound("not.found", "species not found");

        var isSpeciesExist = species.FirstOrDefault(s => s == command.AnimalType.SpeciesId);
        if (isSpeciesExist == Guid.Empty)
            return Error.NotFound("not.found", "this species not found");

        var breeds = await _speciesContracts.GetBreedsBySpeciesId(isSpeciesExist, cancellationToken);
        if (!breeds.Any())
            return Error.NotFound("not.found", "breeds not found");

        var isBreedExist = breeds.FirstOrDefault(b => b == command.AnimalType.BreedId);
        if (isBreedExist == Guid.Empty)
            return Error.NotFound("not.found", "this breed not found");

        var animalType = new AnimalType(SpeciesId.Create(command.AnimalType.SpeciesId), command.AnimalType.BreedId);

        var animalSex = AnimalSex.Create(command.AnimalSex).Value;

        var history = History.Create(command.History.ArriveTime, command.History.LastOwner, command.History.From).Value;

        var temperament = command.Temperament is not null 
            ? Temperament.Create(
                command.Temperament.AggressionLevel,
                command.Temperament.Friendliness,
                command.Temperament.ActivityLevel,
                command.Temperament.GoodWithKids,
                command.Temperament.GoodWithPeople,
                command.Temperament.GoodWithOtherAnimals)
            : null;
        
        if (temperament is not null && temperament.IsFailure)
        {
            return temperament.Errors;
        }

        var medicalInfo = command.MedicalInfo is not null
            ? MedicalInfo.Create(
                command.MedicalInfo.IsSpayedNeutered,
                command.MedicalInfo.IsVaccinated,
                command.MedicalInfo.LastVaccinationDate,
                command.MedicalInfo.HasChronicDiseases,
                command.MedicalInfo.MedicalNotes,
                command.MedicalInfo.RequiresSpecialDiet,
                command.MedicalInfo.HasAllergies)
            : null;
        
        if (medicalInfo is not null && medicalInfo.IsFailure)
        {
            return medicalInfo.Errors;
        }

        var requisites =
            new ValueObjectList<Requisite>(command.Requisites
                .Select(r => Requisite.Create(r.Title, r.Description).Value).ToList());

        var pet = new Pet(
            petId,
            name,
            petPhysicCharacteristics,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            animalSex,
            history,
            temperament?.Value,
            medicalInfo?.Value,
            requisites);
        return pet;
    }
}