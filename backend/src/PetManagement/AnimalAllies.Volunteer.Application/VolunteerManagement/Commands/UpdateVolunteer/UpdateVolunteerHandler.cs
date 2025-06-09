using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;

public class UpdateVolunteerHandler : ICommandHandler<UpdateVolunteerCommand, VolunteerId>
{
    private readonly IVolunteerRepository _repository;
    private readonly ILogger<UpdateVolunteerHandler> _logger;
    private readonly IValidator<UpdateVolunteerCommand> _validator;
    
    public UpdateVolunteerHandler(
        IVolunteerRepository repository, 
        ILogger<UpdateVolunteerHandler> logger,
        IValidator<UpdateVolunteerCommand> validator)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<VolunteerId>> Handle(
        UpdateVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }
        
        var volunteer = await _repository.GetById(VolunteerId.Create(command.Id),cancellationToken);

        if (volunteer.IsFailure)
            return Errors.General.NotFound();
        
        var phoneNumber = PhoneNumber.Create(command.Dto.PhoneNumber).Value;
        
        var volunteerByPhoneNumber = await _repository.GetByPhoneNumber(phoneNumber,cancellationToken);

        if (!volunteerByPhoneNumber.IsFailure)
            return Errors.Volunteer.AlreadyExist();
        
        var fullName = FullName.Create(
            command.Dto.FullName.FirstName,
            command.Dto.FullName.SecondName,
            command.Dto.FullName.Patronymic).Value;
        var workExperience = WorkExperience.Create(command.Dto.WorkExperience).Value;


        volunteer.Value.UpdateInfo(
            fullName,
            phoneNumber,
            workExperience);
        
        _logger.LogInformation("volunteer with title {fullName} and id {volunteerId} updated ", fullName, command.Id);
        
        return await _repository.Save(volunteer.Value, cancellationToken);
    }
}