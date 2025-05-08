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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestorePet;

public class RestorePetHandler(
    ILogger<RestorePetHandler> logger,
    IValidator<RestorePetCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IVolunteerRepository volunteerRepository) : ICommandHandler<RestorePetCommand, PetId>
{
    private readonly ILogger<RestorePetHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<RestorePetCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<PetId>> Handle(
        RestorePetCommand command, CancellationToken cancellationToken = default)
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

        Result result = volunteer.Value.RestorePet(petId);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("pet with id {petId} has been restored", command.PetId);

        return petId;
    }
}