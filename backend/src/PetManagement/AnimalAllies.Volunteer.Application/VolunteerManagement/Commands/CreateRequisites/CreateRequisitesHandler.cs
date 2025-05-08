using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;

public class CreateRequisitesHandler(
    IVolunteerRepository repository,
    ILogger<UpdateVolunteerHandler> logger,
    IValidator<CreateRequisitesCommand> validator) : ICommandHandler<CreateRequisitesCommand, VolunteerId>
{
    private readonly ILogger<UpdateVolunteerHandler> _logger = logger;
    private readonly IVolunteerRepository _repository = repository;
    private readonly IValidator<CreateRequisitesCommand> _validator = validator;

    public async Task<Result<VolunteerId>> Handle(
        CreateRequisitesCommand command,
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

        IEnumerable<Requisite> requisites = command.RequisiteDtos
            .Select(x => Requisite.Create(x.Title, x.Description).Value);

        ValueObjectList<Requisite> volunteerRequisites = new([.. requisites]);

        volunteer.Value.UpdateRequisites(volunteerRequisites);

        Result<VolunteerId> result = await _repository.Save(volunteer.Value, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("volunteer with id {volunteerId} updated volunteer requisites", command.Id);

        return result;
    }
}