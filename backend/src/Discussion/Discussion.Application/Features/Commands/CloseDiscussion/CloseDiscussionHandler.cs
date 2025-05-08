using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Application.Repository;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.CloseDiscussion;

public class CloseDiscussionHandler(
    ILogger<CloseDiscussionHandler> logger,
    [FromKeyedServices(Constraints.Context.Discussion)]
    IUnitOfWork unitOfWork,
    IValidator<CloseDiscussionCommand> validator,
    IDiscussionRepository repository,
    IPublisher publisher) : ICommandHandler<CloseDiscussionCommand, DiscussionId>
{
    private readonly ILogger<CloseDiscussionHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IDiscussionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CloseDiscussionCommand> _validator = validator;

    public async Task<Result<DiscussionId>> Handle(
        CloseDiscussionCommand command, CancellationToken cancellationToken = default)
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
            DiscussionId discussionId = DiscussionId.Create(command.DiscussionId);

            Result<Domain.Aggregate.Discussion> discussion =
                await _repository.GetById(discussionId, cancellationToken).ConfigureAwait(false);
            if (discussion.IsFailure)
            {
                return discussion.Errors;
            }

            Result result = discussion.Value.CloseDiscussion(command.UserId);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "user with id {userId} closed discussion with id {discussionId}",
                command.UserId, command.DiscussionId);

            return discussionId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot close discussion");

            return Error.Failure("cannot.close.discussion", "Cannot close discussion");
        }
    }
}