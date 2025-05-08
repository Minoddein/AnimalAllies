using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;

public class CreateVolunteerHandler(
    IVolunteerRepository repository,
    ILogger<CreateVolunteerHandler> logger,
    IValidator<CreateVolunteerCommand> validator) : ICommandHandler<CreateVolunteerCommand, VolunteerId>
{
    private readonly ILogger<CreateVolunteerHandler> _logger = logger;
    private readonly IVolunteerRepository _repository = repository;
    private readonly IValidator<CreateVolunteerCommand> _validator = validator;

    public async Task<Result<VolunteerId>> Handle(
        CreateVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        PhoneNumber phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        Email email = Email.Create(command.Email).Value;

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerByPhoneNumber =
            await _repository.GetByPhoneNumber(phoneNumber, cancellationToken).ConfigureAwait(false);
        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerByEmail =
            await _repository.GetByEmail(email, cancellationToken).ConfigureAwait(false);

        if (!volunteerByPhoneNumber.IsFailure || !volunteerByEmail.IsFailure)
        {
            return Errors.Volunteer.AlreadyExist();
        }

        FullName fullName = FullName
            .Create(command.FullName.FirstName, command.FullName.SecondName, command.FullName.Patronymic).Value;
        VolunteerDescription description = VolunteerDescription.Create(command.Description).Value;
        WorkExperience workExperience = WorkExperience.Create(command.WorkExperience).Value;

        IEnumerable<Requisite> requisites = command.Requisites
            .Select(x => Requisite.Create(x.Title, x.Description).Value);

        ValueObjectList<Requisite> volunteerRequisites = new([.. requisites]);

        VolunteerId volunteerId = VolunteerId.NewGuid();

        Domain.VolunteerManagement.Aggregate.Volunteer volunteerEntity = new(
            volunteerId,
            fullName,
            email,
            description,
            workExperience,
            phoneNumber,
            volunteerRequisites);

        Result<VolunteerId> result = await _repository.Create(volunteerEntity, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created volunteer {fullName} with id {volunteerId}", fullName, volunteerId.Id);

        return result;
    }
}