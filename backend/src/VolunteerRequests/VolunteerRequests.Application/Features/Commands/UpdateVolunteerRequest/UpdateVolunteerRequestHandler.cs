using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Features.Commands.UpdateVolunteerRequest;

public class UpdateVolunteerRequestHandler : ICommandHandler<UpdateVolunteerRequestCommand, VolunteerRequestId>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateVolunteerRequestHandler> _logger;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IValidator<UpdateVolunteerRequestCommand> _validator;
    private readonly IPublisher _publisher;

    public UpdateVolunteerRequestHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork,
        ILogger<UpdateVolunteerRequestHandler> logger,
        IVolunteerRequestsRepository repository,
        IValidator<UpdateVolunteerRequestCommand> validator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _repository = repository;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result<VolunteerRequestId>> Handle(
        UpdateVolunteerRequestCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );
        try
        {
            var volunteerRequestId = VolunteerRequestId.Create(command.VolunteerRequestId);
            var volunteerRequest = await _repository.GetById(volunteerRequestId, cancellationToken);
            if (volunteerRequest.IsFailure || volunteerRequest.Value.UserId != command.UserId)
                return volunteerRequest.Errors;

            var volunteerInfo = InitVolunteerInfo(command, volunteerRequest.Value);
            if(volunteerInfo.IsFailure)
                return volunteerInfo.Errors;

            var result = volunteerRequest.Value.UpdateVolunteerRequest(volunteerInfo.Value);
            if (result.IsFailure)
                return result.Errors;

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();

            _logger.LogInformation("volunteer request with id {id} was updated", command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot update volunteer request");
            
            return Error.Failure("Fail.to.update.volunteer.request",
                "Cannot update volunteer request");
        }
    }

    private Result<VolunteerInfo> InitVolunteerInfo(
    UpdateVolunteerRequestCommand command, 
    VolunteerRequest existingRequest)
{
    var currentInfo = existingRequest.VolunteerInfo;
    
    var fullNameResult = command.FullNameDto != null
        ? FullName.Create(
            command.FullNameDto.FirstName ?? currentInfo.FullName.FirstName,
            command.FullNameDto.SecondName ?? currentInfo.FullName.SecondName,
            command.FullNameDto.Patronymic ?? currentInfo.FullName.Patronymic)
        : Result<FullName>.Success(currentInfo.FullName);

    if (fullNameResult.IsFailure)
        return fullNameResult.Errors;
    
    var emailResult = command.Email != null 
        ? Email.Create(command.Email)
        : Result<Email>.Success(currentInfo.Email);

    if (emailResult.IsFailure)
        return emailResult.Errors;
    
    var phoneNumberResult = command.PhoneNumber != null 
        ? PhoneNumber.Create(command.PhoneNumber)
        : Result<PhoneNumber>.Success(currentInfo.PhoneNumber);

    if (phoneNumberResult.IsFailure)
        return phoneNumberResult.Errors;
    
    var workExperienceResult = command.WorkExperience.HasValue 
        ? WorkExperience.Create(command.WorkExperience.Value)
        : Result<WorkExperience>.Success(currentInfo.WorkExperience);

    if (workExperienceResult.IsFailure)
        return workExperienceResult.Errors;
    
    var volunteerDescriptionResult = command.VolunteerDescription != null 
        ? VolunteerDescription.Create(command.VolunteerDescription)
        : Result<VolunteerDescription>.Success(currentInfo.VolunteerDescription);

    if (volunteerDescriptionResult.IsFailure)
        return volunteerDescriptionResult.Errors;

    var volunteerInfo = new VolunteerInfo(
        fullNameResult.Value,
        emailResult.Value,
        phoneNumberResult.Value,
        workExperienceResult.Value,
        volunteerDescriptionResult.Value);

    return Result<VolunteerInfo>.Success(volunteerInfo);
}
}