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

namespace VolunteerRequests.Application.Features.Commands.SendRequestForRevision;

public class SendRequestForRevisionHandler(
    ILogger<SendRequestForRevisionHandler> logger,
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    IValidator<SendRequestForRevisionCommand> validator,
    IVolunteerRequestsRepository repository,
    IPublisher publisher) : ICommandHandler<SendRequestForRevisionCommand, VolunteerRequestId>
{
    private readonly ILogger<SendRequestForRevisionHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<SendRequestForRevisionCommand> _validator = validator;

    public async Task<Result<VolunteerRequestId>> Handle(
        SendRequestForRevisionCommand command, CancellationToken cancellationToken = default)
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

            RejectionComment rejectComment = RejectionComment.Create(command.RejectionComment).Value;

            Result result = volunteerRequest.Value.SendRequestForRevision(rejectComment);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("volunteer request with id {id} sent to revision", command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Something went wrong with sending request for revision");

            return Error.Failure(
                "fail.to.send.to.revision.request",
                "Something went wrong with sending request for revision");
        }
    }
}