using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;

public class DeleteVolunteerHandler(
    IVolunteerRepository repository,
    ILogger<DeleteVolunteerHandler> logger,
    IValidator<DeleteVolunteerCommand> validator,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteVolunteerCommand, VolunteerId>
{
    private readonly ILogger<DeleteVolunteerHandler> _logger = logger;
    private readonly IVolunteerRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeleteVolunteerCommand> _validator = validator;

    public async Task<Result<VolunteerId>> Handle(
        DeleteVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteer =
            await _repository.GetById(VolunteerId.Create(command.Id), cancellationToken).ConfigureAwait(false);

        if (volunteer.IsFailure)
        {
            return Errors.General.NotFound();
        }

        volunteer.Value.Delete();

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("volunteer with id {volunteerId} deleted ", command.Id);

        return volunteer.Value.Id;
    }
}