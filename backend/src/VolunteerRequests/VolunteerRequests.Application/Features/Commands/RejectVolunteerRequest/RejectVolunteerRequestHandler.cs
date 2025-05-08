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
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Commands.RejectVolunteerRequest;

public class RejectVolunteerRequestHandler(
    ILogger<RejectVolunteerRequestHandler> logger,
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    IVolunteerRequestsRepository repository,
    IValidator<RejectVolunteerRequestCommand> validator,
    IPublisher publisher) : ICommandHandler<RejectVolunteerRequestCommand, string>
{
    private readonly ILogger<RejectVolunteerRequestHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<RejectVolunteerRequestCommand> _validator = validator;

    public async Task<Result<string>> Handle(
        RejectVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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

            if (volunteerRequest.Value.AdminId != command.AdminId)
            {
                return Error.Failure(
                    "access.denied",
                    "this request is under consideration by another admin");
            }

            RejectionComment rejectionComment = RejectionComment.Create(command.RejectionComment).Value;

            Result rejectResult = volunteerRequest.Value.RejectRequest(rejectionComment);
            if (rejectResult.IsFailure)
            {
                return rejectResult.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "Volunteer request with id {volunteerRequestId} was rejected",
                command.VolunteerRequestId);

            return rejectionComment.Value;
        }
        catch (Exception)
        {
            _logger.LogError("Fail to reject request");

            return Error.Failure("fail.reject.request", "Fail to reject request");
        }
    }
}