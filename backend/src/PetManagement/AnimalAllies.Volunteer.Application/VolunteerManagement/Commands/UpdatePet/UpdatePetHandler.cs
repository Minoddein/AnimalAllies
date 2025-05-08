using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;

public class UpdatePetHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<UpdatePetHandler> logger,
    IDateTimeProvider dateTimeProvider,
    IValidator<UpdatePetCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    ISpeciesContracts speciesContracts) : ICommandHandler<UpdatePetCommand, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<UpdatePetHandler> _logger = logger;
    private readonly ISpeciesContracts _speciesContracts = speciesContracts;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<UpdatePetCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<Guid>> Handle(
        UpdatePetCommand command,
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

        PetId petId = PetId.Create(command.PetId);

        Result<Pet> petIsExist = volunteerResult.Value.GetPetById(petId);
        if (petIsExist.IsFailure)
        {
            return petIsExist.Errors;
        }

        Name name = Name.Create(command.Name).Value;

        PhoneNumber phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;

        HelpStatus helpStatus = HelpStatus.Create(command.HelpStatus).Value;

        PetPhysicCharacteristics petPhysicCharacteristics = PetPhysicCharacteristics.Create(
            command.PetPhysicCharacteristicsDto.Color,
            command.PetPhysicCharacteristicsDto.HealthInformation,
            command.PetPhysicCharacteristicsDto.Weight,
            command.PetPhysicCharacteristicsDto.Height,
            command.PetPhysicCharacteristicsDto.IsCastrated,
            command.PetPhysicCharacteristicsDto.IsVaccinated).Value;

        PetDetails petDetails = PetDetails.Create(
            command.PetDetailsDto.Description,
            DateOnly.FromDateTime(command.PetDetailsDto.BirthDate),
            _dateTimeProvider.UtcNow).Value;

        Address address = Address.Create(
            command.AddressDto.Street,
            command.AddressDto.City,
            command.AddressDto.State,
            command.AddressDto.ZipCode).Value;

        SpeciesId speciesId = SpeciesId.Create(command.AnimalTypeDto.SpeciesId);

        List<Guid> species = await _speciesContracts.GetSpecies(cancellationToken).ConfigureAwait(false);
        if (species.Count == 0)
        {
            return Errors.General.NotFound();
        }

        Guid isSpeciesExist = species.FirstOrDefault(s => s == command.AnimalTypeDto.SpeciesId);
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

        Guid isBreedExist = breeds.FirstOrDefault(b => b == command.AnimalTypeDto.BreedId);
        if (isBreedExist == Guid.Empty)
        {
            return Errors.General.NotFound();
        }

        AnimalType animalType = new(speciesId, command.AnimalTypeDto.BreedId);

        ValueObjectList<Requisite> requisites =
            new([.. command.RequisiteDtos.Select(r => Requisite.Create(r.Title, r.Description).Value)]);

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

        Result result = volunteerResult.Value.UpdatePet(
            petId,
            name,
            petPhysicCharacteristics,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            requisites);

        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("added pet with id {petId} to volunteer with id {volunteerId}", petId.Id,
            volunteerResult.Value.Id.Id);

        return pet.Id.Id;
    }
}