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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestoreVolunteer;

public class RestoreVolunteerHandler(
    ILogger<RestoreVolunteerHandler> logger,
    IValidator<RestoreVolunteerCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IVolunteerRepository volunteerRepository) : ICommandHandler<RestoreVolunteerCommand, VolunteerId>
{
    private readonly ILogger<RestoreVolunteerHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<RestoreVolunteerCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<VolunteerId>> Handle(
        RestoreVolunteerCommand command, CancellationToken cancellationToken = default)
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

        volunteer.Value.Restore();

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("volunteer with id {volunteerId} has been restored", command.VolunteerId);

        return volunteerId;
    }
}