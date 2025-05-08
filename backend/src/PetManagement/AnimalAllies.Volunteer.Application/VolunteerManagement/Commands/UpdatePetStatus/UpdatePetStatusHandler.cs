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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePetStatus;

public class UpdatePetStatusHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<UpdatePetStatusHandler> logger,
    IValidator<UpdatePetStatusCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePetStatusCommand, PetId>
{
    private readonly ILogger<UpdatePetStatusHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<UpdatePetStatusCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<PetId>> Handle(
        UpdatePetStatusCommand command,
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

        Result<HelpStatus> helpStatus = HelpStatus.Create(command.HelpStatus);
        if (helpStatus.IsFailure)
        {
            return helpStatus.Errors;
        }

        Result result = volunteer.Value.UpdatePetStatus(petId, helpStatus.Value);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        _logger.LogInformation(
            "Update status to pet with id {petId} from volunteer with id {volunteerId}",
            petId.Id, volunteerId.Id);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        return petId;
    }
}