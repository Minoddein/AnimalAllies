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

            var volunteerInfo = InitVolunteerInfo(command).Value;

            var result = volunteerRequest.Value.UpdateVolunteerRequest(volunteerInfo);
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
        UpdateVolunteerRequestCommand command)
    {
        var fullName = FullName.Create(
            command.FullNameDto.FirstName,
            command.FullNameDto.SecondName,
            command.FullNameDto.Patronymic).Value;

        var email = Email.Create(command.Email).Value;
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        var workExperience = WorkExperience.Create(command.WorkExperience).Value;
        var volunteerDescription = VolunteerDescription.Create(command.VolunteerDescription).Value;
        var socialNetworks = command.SocialNetworkDtos
            .Select(s => SocialNetwork.Create(s.Title, s.Url).Value);

        var volunteerInfo = new VolunteerInfo(
            fullName, email, phoneNumber, workExperience, volunteerDescription, socialNetworks);
        
        return volunteerInfo;
    }
}