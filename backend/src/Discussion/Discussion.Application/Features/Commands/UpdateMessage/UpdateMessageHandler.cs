using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Application.Repository;
using Discussion.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.UpdateMessage;

public class UpdateMessageHandler(
    ILogger<UpdateMessageHandler> logger,
    [FromKeyedServices(Constraints.Context.Discussion)]
    IUnitOfWork unitOfWork,
    IValidator<UpdateMessageCommand> validator,
    IDiscussionRepository repository,
    IPublisher publisher) : ICommandHandler<UpdateMessageCommand, MessageId>
{
    private readonly ILogger<UpdateMessageHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IDiscussionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<UpdateMessageCommand> _validator = validator;

    public async Task<Result<MessageId>> Handle(
        UpdateMessageCommand command, CancellationToken cancellationToken = default)
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

            MessageId messageId = MessageId.Create(command.MessageId);
            Text text = Text.Create(command.Text).Value;

            Result result = discussion.Value.EditComment(command.UserId, messageId, text);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "user with id {userId} edit message with id {messageId}" +
                " from discussion with id {discussionId}",
                command.UserId, command.MessageId, command.DiscussionId);

            return messageId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot update message");

            return Error.Failure("cannot.update.message", "Cannot update message");
        }
    }
}