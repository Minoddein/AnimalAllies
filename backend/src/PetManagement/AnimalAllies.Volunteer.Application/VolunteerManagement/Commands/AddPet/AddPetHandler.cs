using AnimalAllies.Core.Abstractions;
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
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;

public class AddPetHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<AddPetHandler> logger,
    IDateTimeProvider dateTimeProvider,
    IValidator<AddPetCommand> validator,
    ISpeciesContracts speciesContracts) : ICommandHandler<AddPetCommand, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<AddPetHandler> _logger = logger;
    private readonly ISpeciesContracts _speciesContracts = speciesContracts;
    private readonly IValidator<AddPetCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<Guid>> Handle(
        AddPetCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerResult = await _volunteerRepository.GetById(
            VolunteerId.Create(command.VolunteerId), cancellationToken).ConfigureAwait(false);

        if (volunteerResult.IsFailure)
        {
            return volunteerResult.Errors;
        }

        PetId petId = PetId.NewGuid();

        Name name = Name.Create(command.Name).Value;

        PhoneNumber phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;

        HelpStatus helpStatus = HelpStatus.Create(command.HelpStatus).Value;

        PetPhysicCharacteristics petPhysicCharacteristics = PetPhysicCharacteristics.Create(
            command.PetPhysicCharacteristics.Color,
            command.PetPhysicCharacteristics.HealthInformation,
            command.PetPhysicCharacteristics.Weight,
            command.PetPhysicCharacteristics.Height,
            command.PetPhysicCharacteristics.IsCastrated,
            command.PetPhysicCharacteristics.IsVaccinated).Value;

        PetDetails petDetails = PetDetails.Create(
            command.PetDetails.Description,
            DateOnly.FromDateTime(command.PetDetails.BirthDate),
            _dateTimeProvider.UtcNow).Value;

        Address address = Address.Create(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.ZipCode).Value;

        List<Guid> species = await _speciesContracts.GetSpecies(cancellationToken).ConfigureAwait(false);
        if (species.Count == 0)
        {
            return Errors.General.NotFound();
        }

        Guid isSpeciesExist = species.FirstOrDefault(s => s == command.AnimalType.SpeciesId);
        if (isSpeciesExist == Guid.Empty)
        {
            return Errors.General.NotFound();
        }

        List<Guid> breeds = await _speciesContracts.GetBreedsBySpeciesId(isSpeciesExist, cancellationToken)
            .ConfigureAwait(false);
        if (breeds.Count == 0)
        {
            return Errors.General.NotFound();
        }

        Guid isBreedExist = breeds.FirstOrDefault(b => b == command.AnimalType.BreedId);
        if (isBreedExist == Guid.Empty)
        {
            return Errors.General.NotFound();
        }

        AnimalType animalType = new(SpeciesId.Create(command.AnimalType.SpeciesId), command.AnimalType.BreedId);

        ValueObjectList<Requisite> requisites =
            new([.. command.Requisites.Select(r => Requisite.Create(r.Title, r.Description).Value)]);

        Pet pet = new(
            petId,
            name,
            petPhysicCharacteristics,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            requisites);

        volunteerResult.Value.AddPet(pet);

        await _volunteerRepository.Save(volunteerResult.Value, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("added pet with id {petId} to volunteer with id {volunteerId}", petId.Id,
            volunteerResult.Value.Id.Id);

        return pet.Id.Id;
    }
}