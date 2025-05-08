using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetSoft;

public class DeletePetSoftHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<DeletePetSoftHandler> logger,
    IValidator<DeletePetSoftCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeletePetSoftCommand, PetId>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<DeletePetSoftHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeletePetSoftCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<PetId>> Handle(DeletePetSoftCommand command, CancellationToken cancellationToken = default)
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

        Result result = volunteer.Value.DeletePetSoft(petId, _dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        _logger.LogInformation(
            "Soft deleted pet with id {petId} from volunteer with id {volunteerId}",
            petId.Id, volunteerId.Id);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        return petId;
    }
}