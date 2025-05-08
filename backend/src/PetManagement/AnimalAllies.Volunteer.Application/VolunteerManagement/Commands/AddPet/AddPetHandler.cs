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

        var petId = PetId.NewGuid();
        
        var name = Name.Create(command.Name).Value;
        
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        
        var helpStatus = HelpStatus.Create(command.HelpStatus).Value;
        
        var petPhysicCharacteristics = PetPhysicCharacteristics.Create(
            command.PetPhysicCharacteristics.Color,
            command.PetPhysicCharacteristics.HealthInformation,
            command.PetPhysicCharacteristics.Weight,
            command.PetPhysicCharacteristics.Height,
            command.PetPhysicCharacteristics.IsCastrated,
            command.PetPhysicCharacteristics.IsVaccinated).Value;

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
            return Errors.General.NotFound();

        var isSpeciesExist = species.FirstOrDefault(s => s == command.AnimalType.SpeciesId);
        if (isSpeciesExist == Guid.Empty)
            return Errors.General.NotFound();

        var breeds = await _speciesContracts.GetBreedsBySpeciesId(isSpeciesExist,cancellationToken);
        if (!breeds.Any())
            return Errors.General.NotFound();

        var isBreedExist = breeds.FirstOrDefault(b => b == command.AnimalType.BreedId);
        if (isBreedExist == Guid.Empty)
            return Errors.General.NotFound();
        
        var animalType = new AnimalType(SpeciesId.Create(command.AnimalType.SpeciesId), command.AnimalType.BreedId);

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
            requisites);

        volunteerResult.Value.AddPet(pet);

        await _volunteerRepository.Save(volunteerResult.Value, cancellationToken);
        
        _logger.LogInformation("added pet with id {petId} to volunteer with id {volunteerId}", petId.Id, volunteerResult.Value.Id.Id);
        
        return pet.Id.Id;
    }
}