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
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Domain.Events;

namespace VolunteerRequests.Application.Features.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestHandler(
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    ILogger<CreateVolunteerRequestHandler> logger,
    IValidator<CreateVolunteerRequestCommand> validator,
    IVolunteerRequestsRepository repository,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateVolunteerRequestCommand, VolunteerRequestId>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<CreateVolunteerRequestHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateVolunteerRequestCommand> _validator = validator;

    public async Task<Result<VolunteerRequestId>> Handle(
        CreateVolunteerRequestCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? resultValidator =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!resultValidator.IsValid)
        {
            return resultValidator.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            await _publisher.PublishDomainEvent(
                new ProhibitionOnVolunteerRequestCheckedEvent(command.UserId), cancellationToken).ConfigureAwait(false);

            Result<VolunteerRequest> volunteerRequestResult = InitVolunteerRequest(command);
            if (volunteerRequestResult.IsFailure)
            {
                return volunteerRequestResult.Errors;
            }

            VolunteerRequest volunteerRequest = volunteerRequestResult.Value;

            await _repository.Create(volunteerRequest, cancellationToken).ConfigureAwait(false);

            await _publisher.PublishDomainEvents(volunteerRequest, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "user with id {userId} created volunteer request with id {volunteerRequestId}",
                command.UserId,
                volunteerRequest.Id);

            return volunteerRequest.Id;
        }
        catch (AccountBannedException)
        {
            _logger.LogError($"User was prohibited for creating request with id {command.UserId}");

            return Error.Failure(
                "access.denied",
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
        FullName fullName = FullName.Create(
            command.FullNameDto.FirstName,
            command.FullNameDto.SecondName,
            command.FullNameDto.Patronymic).Value;

        Email email = Email.Create(command.Email).Value;
        PhoneNumber phoneNumber = PhoneNumber.Create(command.PhoneNumber).Value;
        WorkExperience workExperience = WorkExperience.Create(command.WorkExperience).Value;
        VolunteerDescription volunteerDescription = VolunteerDescription.Create(command.VolunteerDescription).Value;
        IEnumerable<SocialNetwork> socialNetworks = command
            .SocialNetworkDtos.Select(s => SocialNetwork.Create(s.Title, s.Url).Value);

        VolunteerInfo volunteerInfo = new(
            fullName,
            email,
            phoneNumber,
            workExperience,
            volunteerDescription,
            socialNetworks);

        CreatedAt createdAt = CreatedAt.Create(_dateTimeProvider.UtcNow).Value;
        VolunteerRequestId volunteerRequestId = VolunteerRequestId.NewGuid();

        Result<VolunteerRequest> volunteerRequest = VolunteerRequest.Create(
            volunteerRequestId, createdAt, volunteerInfo, command.UserId);

        if (volunteerRequest.IsFailure)
        {
            return volunteerRequest.Errors;
        }

        return volunteerRequest.Value;
    }
}