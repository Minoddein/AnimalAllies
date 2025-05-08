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
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Features.Commands.UpdateVolunteerRequest;

public class UpdateVolunteerRequestHandler(
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    ILogger<UpdateVolunteerRequestHandler> logger,
    IVolunteerRequestsRepository repository,
    IValidator<UpdateVolunteerRequestCommand> validator,
    IPublisher publisher) : ICommandHandler<UpdateVolunteerRequestCommand, VolunteerRequestId>
{
    private readonly ILogger<UpdateVolunteerRequestHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<UpdateVolunteerRequestCommand> _validator = validator;

    public async Task<Result<VolunteerRequestId>> Handle(
        UpdateVolunteerRequestCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );
        try
        {
            VolunteerRequestId volunteerRequestId = VolunteerRequestId.Create(command.VolunteerRequestId);
            Result<VolunteerRequest> volunteerRequest =
                await _repository.GetById(volunteerRequestId, cancellationToken).ConfigureAwait(false);
            if (volunteerRequest.IsFailure || volunteerRequest.Value.UserId != command.UserId)
            {
                return volunteerRequest.Errors;
            }

            VolunteerInfo volunteerInfo = InitVolunteerInfo(command).Value;

            Result result = volunteerRequest.Value.UpdateVolunteerRequest(volunteerInfo);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("volunteer request with id {id} was updated", command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot update volunteer request");

            return Error.Failure(
                "Fail.to.update.volunteer.request",
                "Cannot update volunteer request");
        }
    }

    private static Result<VolunteerInfo> InitVolunteerInfo(
        UpdateVolunteerRequestCommand command)
    {
        FullName fullName = FullName.Create(
            command.FullNameDto.FirstName,
            command.FullNameDto.SecondName,
            command.FullNameDto.Patronymic).Value;

        Email email = Email.Create(command.Email).Value;
        PhoneNumber phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        WorkExperience workExperience = WorkExperience.Create(command.WorkExperience).Value;
        VolunteerDescription volunteerDescription = VolunteerDescription.Create(command.VolunteerDescription).Value;
        IEnumerable<SocialNetwork> socialNetworks = command.SocialNetworkDtos
            .Select(s => SocialNetwork.Create(s.Title, s.Url).Value);

        VolunteerInfo volunteerInfo = new(
            fullName, email, phoneNumber, workExperience, volunteerDescription, socialNetworks);

        return volunteerInfo;
    }
}