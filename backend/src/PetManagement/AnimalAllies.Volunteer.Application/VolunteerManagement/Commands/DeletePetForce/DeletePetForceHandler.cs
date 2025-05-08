using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Providers;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetForce;

public class DeletePetForceHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<DeletePetForceHandler> logger,
    IValidator<DeletePetForceCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IFileProvider fileProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeletePetForceCommand, PetId>
{
    private const string BUCKET_NAME = "photos";
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IFileProvider _fileProvider = fileProvider;
    private readonly ILogger<DeletePetForceHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeletePetForceCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<PetId>> Handle(
        DeletePetForceCommand command,
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
        Result<Pet> pet = volunteer.Value.GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        Result result = volunteer.Value.DeletePetForce(petId, _dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        List<FileInfo> petPreviousPhotos =
            [.. pet.Value.PetPhotoDetails.Select(f => new FileInfo(f.Path, BUCKET_NAME))];

        if (petPreviousPhotos.Count != 0)
        {
            petPreviousPhotos.ForEach(f => _fileProvider.RemoveFile(f, cancellationToken));
        }

        _logger.LogInformation(
            "Soft deleted pet with id {petId} from volunteer with id {volunteerId}",
            petId.Id, volunteerId.Id);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        return petId;
    }
}