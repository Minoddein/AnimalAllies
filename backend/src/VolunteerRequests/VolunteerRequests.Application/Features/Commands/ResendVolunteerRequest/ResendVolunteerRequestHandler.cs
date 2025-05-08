using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Features.Commands.ResendVolunteerRequest;

public class ResendVolunteerRequestHandler(
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    ILogger<ResendVolunteerRequestHandler> logger,
    IVolunteerRequestsRepository repository,
    IValidator<ResendVolunteerRequestCommand> validator,
    IPublisher publisher) : ICommandHandler<ResendVolunteerRequestCommand, VolunteerRequestId>
{
    private readonly ILogger<ResendVolunteerRequestHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<ResendVolunteerRequestCommand> _validator = validator;

    public async Task<Result<VolunteerRequestId>> Handle(
        ResendVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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
            if (volunteerRequest.IsFailure)
            {
                return volunteerRequest.Errors;
            }

            if (volunteerRequest.Value.UserId != command.UserId)
            {
                return Error.Failure("access.conflict", "Request belong another user!");
            }

            Result result = volunteerRequest.Value.ResendVolunteerRequest();
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "user with id {userId} resent volunteer request with id {requestId}",
                command.UserId, command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot resend volunteer request");

            return Error.Failure(
                "Fail.to.resend.volunteer.request",
                "Cannot resend volunteer request");
        }
    }
}