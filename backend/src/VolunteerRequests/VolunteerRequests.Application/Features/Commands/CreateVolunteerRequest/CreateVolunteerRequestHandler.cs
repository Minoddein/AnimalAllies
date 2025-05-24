using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Exceptions;
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
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.Features.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestHandler: ICommandHandler<CreateVolunteerRequestCommand,VolunteerRequestId>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateVolunteerRequestHandler> _logger;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IValidator<CreateVolunteerRequestCommand> _validator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public CreateVolunteerRequestHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork,
        ILogger<CreateVolunteerRequestHandler> logger,
        IValidator<CreateVolunteerRequestCommand> validator,
        IVolunteerRequestsRepository repository,
        IPublisher publisher,
        IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _validator = validator;
        _repository = repository;
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
    }
    //TODO: Возможно удалить social networks при подачи заявлений
    public async Task<Result<VolunteerRequestId>> Handle(
        CreateVolunteerRequestCommand command, CancellationToken cancellationToken = default)
    {
        var resultValidator = await _validator.ValidateAsync(command, cancellationToken);
        if (!resultValidator.IsValid)
            return resultValidator.ToErrorList();

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            await _publisher.PublishDomainEvent(
                new ProhibitionOnVolunteerRequestCheckedEvent(command.UserId), cancellationToken);

            var volunteerRequestResult = InitVolunteerRequest(command);
            if (volunteerRequestResult.IsFailure)
                return volunteerRequestResult.Errors;

            var volunteerRequest = volunteerRequestResult.Value;

            await _repository.Create(volunteerRequest, cancellationToken);

            await _publisher.PublishDomainEvents(volunteerRequest, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();
            
            _logger.LogInformation("user with id {userId} created volunteer request with id {volunteerRequestId}",
                command.UserId,
                volunteerRequest.Id);

            return volunteerRequest.Id;
        }
        catch (AccountBannedException)
        {
            _logger.LogError($"User was prohibited for creating request with id {command.UserId}");
            
            return Error.Failure("access.denied.request.prohibited", 
                $"User was prohibited for creating request with id {command.UserId}");
        }
        catch (Exception)
        {
            _logger.LogError("Fail to create volunteer request");
            
            return Error.Failure("fail.create.request", "Fail to create volunteer request");
        }
    }

    private Result<VolunteerRequest> InitVolunteerRequest(CreateVolunteerRequestCommand command)
    {
        var fullName = FullName.Create(
            command.FullNameDto.FirstName,
            command.FullNameDto.SecondName,
            command.FullNameDto.Patronymic).Value;

        var email = Email.Create(command.Email).Value;
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        var workExperience = WorkExperience.Create(command.WorkExperience).Value;
        var volunteerDescription = VolunteerDescription.Create(command.VolunteerDescription).Value;

        var volunteerInfo = new VolunteerInfo(
            fullName,
            email,
            phoneNumber,
            workExperience,
            volunteerDescription);

        var createdAt = CreatedAt.Create(_dateTimeProvider.UtcNow).Value;
        var volunteerRequestId = VolunteerRequestId.NewGuid();

        var volunteerRequest = VolunteerRequest.Create(
            volunteerRequestId, createdAt, volunteerInfo, command.UserId);

        if (volunteerRequest.IsFailure)
            return volunteerRequest.Errors;
        
        return volunteerRequest.Value;
    }
}