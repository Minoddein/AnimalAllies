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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;

public class UpdateVolunteerHandler(
    IVolunteerRepository repository,
    ILogger<UpdateVolunteerHandler> logger,
    IValidator<UpdateVolunteerCommand> validator) : ICommandHandler<UpdateVolunteerCommand, VolunteerId>
{
    private readonly ILogger<UpdateVolunteerHandler> _logger = logger;
    private readonly IVolunteerRepository _repository = repository;
    private readonly IValidator<UpdateVolunteerCommand> _validator = validator;

    public async Task<Result<VolunteerId>> Handle(
        UpdateVolunteerCommand command,
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

        PhoneNumber phoneNumber = PhoneNumber.Create(command.Dto.PhoneNumber).Value;
        Email email = Email.Create(command.Dto.Email).Value;

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerByPhoneNumber =
            await _repository.GetByPhoneNumber(phoneNumber, cancellationToken).ConfigureAwait(false);
        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerByEmail =
            await _repository.GetByEmail(email, cancellationToken).ConfigureAwait(false);

        if (!volunteerByPhoneNumber.IsFailure || !volunteerByEmail.IsFailure)
        {
            return Errors.Volunteer.AlreadyExist();
        }

        FullName fullName = FullName.Create(
            command.Dto.FullName.FirstName,
            command.Dto.FullName.SecondName,
            command.Dto.FullName.Patronymic).Value;
        VolunteerDescription description = VolunteerDescription.Create(command.Dto.Description).Value;
        WorkExperience workExperience = WorkExperience.Create(command.Dto.WorkExperience).Value;

        volunteer.Value.UpdateInfo(
            fullName,
            email,
            phoneNumber,
            description,
            workExperience);

        _logger.LogInformation("volunteer with title {fullName} and id {volunteerId} updated ", fullName, command.Id);

        return await _repository.Save(volunteer.Value, cancellationToken).ConfigureAwait(false);
    }
}