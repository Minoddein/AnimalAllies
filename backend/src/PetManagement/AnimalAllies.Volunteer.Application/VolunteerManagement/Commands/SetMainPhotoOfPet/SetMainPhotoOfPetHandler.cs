using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.SetMainPhotoOfPet;

public class SetMainPhotoOfPetHandler(
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    ILogger<SetMainPhotoOfPetHandler> logger,
    IValidator<SetMainPhotoOfPetCommand> validator,
    IVolunteerRepository volunteerRepository) : ICommandHandler<SetMainPhotoOfPetCommand, PetId>
{
    private readonly ILogger<SetMainPhotoOfPetHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<SetMainPhotoOfPetCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<PetId>> Handle(
        SetMainPhotoOfPetCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        VolunteerId volunteerId = VolunteerId.Create(command.VolunteerId);

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteer =
            await _volunteerRepository.GetById(volunteerId, cancellationToken).ConfigureAwait(false);
        if (volunteer.IsFailure)
        {
            return volunteer.Errors;
        }

        PetId petId = PetId.Create(command.PetId);

        Result<FilePath> filePath = FilePath.Create(command.Path);
        if (filePath.IsFailure)
        {
            return filePath.Errors;
        }

        PetPhoto petPhoto = new(filePath.Value, true);

        Result result = volunteer.Value.SetMainPhotoOfPet(petId, petPhoto);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "set main photo with path {path} to pet with id {petId}",
            command.Path, command.PetId);

        return petId;
    }
}