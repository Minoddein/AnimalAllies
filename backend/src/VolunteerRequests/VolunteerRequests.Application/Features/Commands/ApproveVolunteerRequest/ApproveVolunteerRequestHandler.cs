using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VolunteerRequests.Application.Features.Commands.ApproveVolunteerRequest;

public class ApproveVolunteerRequestHandler(
    ILogger<ApproveVolunteerRequestHandler> logger,
    IValidator<ApproveVolunteerRequestCommand> validator,
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    IVolunteerRequestsRepository repository,
    IPublisher publisher) : ICommandHandler<ApproveVolunteerRequestCommand>
{
    private readonly ILogger<ApproveVolunteerRequestHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<ApproveVolunteerRequestCommand> _validator = validator;

    public async Task<Result> Handle(
        ApproveVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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

            Result<VolunteerRequest> volunteerRequestResult =
                await _repository.GetById(volunteerRequestId, cancellationToken).ConfigureAwait(false);
            if (volunteerRequestResult.IsFailure)
            {
                return volunteerRequestResult.Errors;
            }

            VolunteerRequest volunteerRequest = volunteerRequestResult.Value;

            if (volunteerRequest.AdminId != command.AdminId)
            {
                return Error.Failure(
                    "access.denied",
                    "this request is under consideration by another admin");
            }

            Result approveResult = volunteerRequest.ApproveRequest();
            if (approveResult.IsFailure)
            {
                return approveResult.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("Approved volunteer request with id {id}", command.VolunteerRequestId);

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError("Fail to approve volunteer request");

            return Error.Failure("fail.approve.request", "Fail to approve volunteer request");
        }
    }
}