using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;

public class CreateVolunteerHandler : ICommandHandler<CreateVolunteerCommand, VolunteerId>
{
    private readonly IVolunteerRepository _repository;
    private readonly IValidator<CreateVolunteerCommand> _validator;
    private readonly ILogger<CreateVolunteerHandler> _logger;

    public CreateVolunteerHandler(
        IVolunteerRepository repository,
        ILogger<CreateVolunteerHandler> logger,
        IValidator<CreateVolunteerCommand> validator)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<VolunteerId>> Handle(
        CreateVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }
        
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        var email = Email.Create(command.Email).Value;
        
        var volunteerByPhoneNumber = await _repository.GetByPhoneNumber(phoneNumber,cancellationToken);
        var volunteerByEmail = await _repository.GetByEmail(email,cancellationToken);

        if (!volunteerByPhoneNumber.IsFailure || !volunteerByEmail.IsFailure)
            return Errors.Volunteer.AlreadyExist();
        
        var fullName = FullName.Create(command.FullName.FirstName, command.FullName.SecondName, command.FullName.Patronymic).Value;
        var description = VolunteerDescription.Create(command.Description).Value;
        var workExperience = WorkExperience.Create(command.WorkExperience).Value;
        
        var requisites = command.Requisites
            .Select(x => Requisite.Create(x.Title, x.Description).Value);
        
        var volunteerRequisites = new ValueObjectList<Requisite>(requisites.ToList());
        
        var volunteerId = VolunteerId.NewGuid();
        
        var relation = Relation.Create(command.RelationId).Value;
        
        var volunteerEntity = new Domain.VolunteerManagement.Aggregate.Volunteer(
            volunteerId,
            fullName,
            email,
            description,
            workExperience,
            phoneNumber,
            relation,
            volunteerRequisites);
        
        var result = await _repository.Create(volunteerEntity, cancellationToken);
        
        _logger.LogInformation("Created volunteer {fullName} with id {volunteerId}", fullName, volunteerId.Id);

        return result;
    }
}