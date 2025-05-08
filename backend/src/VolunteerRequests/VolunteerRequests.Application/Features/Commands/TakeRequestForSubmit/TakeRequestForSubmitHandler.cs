using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Contracts;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;

namespace VolunteerRequests.Application.Features.Commands.TakeRequestForSubmit;

public class TakeRequestForSubmitHandler(
    ILogger<TakeRequestForSubmitHandler> logger,
    [FromKeyedServices(Constraints.Context.VolunteerRequests)]
    IUnitOfWork unitOfWork,
    IVolunteerRequestsRepository repository,
    IDiscussionContract discussionContract,
    IValidator<TakeRequestForSubmitCommand> validator,
    IPublisher publisher) : ICommandHandler<TakeRequestForSubmitCommand>
{
    private readonly IDiscussionContract _discussionContract = discussionContract;
    private readonly ILogger<TakeRequestForSubmitHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IVolunteerRequestsRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<TakeRequestForSubmitCommand> _validator = validator;

    public async Task<Result> Handle(
        TakeRequestForSubmitCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
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

            Guid discussionId = await _discussionContract.CreateDiscussionHandler(
                command.AdminId,
                volunteerRequest.Value.UserId,
                command.VolunteerRequestId,
                cancellationToken).ConfigureAwait(false);

            Result result = volunteerRequest.Value.TakeRequestForSubmit(command.AdminId, discussionId);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "admin with id {id} take volunteer request for submit with id {volunteerRequestId}",
                command.AdminId, command.VolunteerRequestId);

            return Result.Success();
        }
        catch (Exception)
        {
            _logger.LogError("Cannot take request for submit");

            return Error.Failure(
                "take.request.for.submit.failure",
                "Something went wrong with take request for submit");
        }
    }
}