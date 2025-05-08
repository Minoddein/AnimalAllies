using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Discussion.Application.Repository;
using Discussion.Domain.Entities;
using Discussion.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.PostMessage;

public class PostMessageHandler(
    ILogger<PostMessageHandler> logger,
    [FromKeyedServices(Constraints.Context.Discussion)]
    IUnitOfWork unitOfWork,
    IValidator<PostMessageCommand> validator,
    IDiscussionRepository repository,
    IDateTimeProvider dateTimeProvider,
    IPublisher publisher) : ICommandHandler<PostMessageCommand, MessageId>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<PostMessageHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IDiscussionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<PostMessageCommand> _validator = validator;

    public async Task<Result<MessageId>> Handle(
        PostMessageCommand command, CancellationToken cancellationToken = default)
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

            MessageId messageId = MessageId.NewGuid();
            Text text = Text.Create(command.Text).Value;
            CreatedAt createdAt = CreatedAt.Create(_dateTimeProvider.UtcNow).Value;
            IsEdited isEdited = new(false);

            Result<Message> message = Message.Create(messageId, text, createdAt, isEdited, command.UserId);
            if (message.IsFailure)
            {
                return message.Errors;
            }

            Result result = discussion.Value.SendComment(message.Value);
            if (result.IsFailure)
            {
                return result.Errors;
            }

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "user with id {userId} post comment to discussion with id {discussionId}",
                command.UserId, discussionId.Id);

            return messageId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot post message in discussion");

            return Error.Failure("fail.to.post.message", "Cannot post message in discussion");
        }
    }
}