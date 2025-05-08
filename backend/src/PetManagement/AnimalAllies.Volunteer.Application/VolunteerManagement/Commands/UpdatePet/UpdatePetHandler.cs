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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;

public class UpdatePetHandler: ICommandHandler<UpdatePetCommand, Guid>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly ISpeciesContracts _speciesContracts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePetHandler> _logger;
    private readonly IValidator<UpdatePetCommand> _validator;

    public UpdatePetHandler(
        IVolunteerRepository volunteerRepository,
        ILogger<UpdatePetHandler> logger,
        IDateTimeProvider dateTimeProvider,
        IValidator<UpdatePetCommand> validator,
        [FromKeyedServices(Constraints.Context.PetManagement)]IUnitOfWork unitOfWork,
        ISpeciesContracts speciesContracts)
    {
        _volunteerRepository = volunteerRepository;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _speciesContracts = speciesContracts;
    }
    

    public async Task<Result<Guid>> Handle(
        UpdatePetCommand command,
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

        var petId = PetId.Create(command.PetId);

        var petIsExist = volunteerResult.Value.GetPetById(petId);
        if (petIsExist.IsFailure)
            return petIsExist.Errors;
        
        var name = Name.Create(command.Name).Value;
        
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        
        var helpStatus = HelpStatus.Create(command.HelpStatus).Value;
        
        var petPhysicCharacteristics = PetPhysicCharacteristics.Create(
            command.PetPhysicCharacteristicsDto.Color,
            command.PetPhysicCharacteristicsDto.HealthInformation,
            command.PetPhysicCharacteristicsDto.Weight,
            command.PetPhysicCharacteristicsDto.Height,
            command.PetPhysicCharacteristicsDto.IsCastrated,
            command.PetPhysicCharacteristicsDto.IsVaccinated).Value;

        var petDetails = PetDetails.Create(
            command.PetDetailsDto.Description,
            DateOnly.FromDateTime(command.PetDetailsDto.BirthDate),
            _dateTimeProvider.UtcNow).Value;

        var address = Address.Create(
            command.AddressDto.Street,
            command.AddressDto.City,
            command.AddressDto.State,
            command.AddressDto.ZipCode).Value;
        
        var speciesId = SpeciesId.Create(command.AnimalTypeDto.SpeciesId);

        var species = await _speciesContracts.GetSpecies(cancellationToken);
        if (!species.Any())
            return Errors.General.NotFound();

        var isSpeciesExist = species.FirstOrDefault(s => s == command.AnimalTypeDto.SpeciesId);
        if (isSpeciesExist == Guid.Empty)
            return Errors.General.NotFound();

        var breeds = await _speciesContracts.GetBreedsBySpeciesId(isSpeciesExist,cancellationToken);
        if (!breeds.Any())
            return Errors.General.NotFound();

        var isBreedExist = breeds.FirstOrDefault(b => b == command.AnimalTypeDto.BreedId);
        if (isBreedExist == Guid.Empty)
            return Errors.General.NotFound();
        
        var animalType = new AnimalType(speciesId, command.AnimalTypeDto.BreedId);

        var requisites =
            new ValueObjectList<Requisite>(command.RequisiteDtos
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

        var result =volunteerResult.Value.UpdatePet(
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
            return result.Errors;

        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("added pet with id {petId} to volunteer with id {volunteerId}", petId.Id, volunteerResult.Value.Id.Id);
        
        return pet.Id.Id;
    }
}