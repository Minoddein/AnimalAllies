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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.PostMessage;

public class PostMessageHandler: ICommandHandler<PostMessageCommand, MessageId>
{
    private readonly ILogger<PostMessageHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<PostMessageCommand> _validator;
    private readonly IDiscussionRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;
    
    public PostMessageHandler(
        ILogger<PostMessageHandler> logger, 
        [FromKeyedServices(Constraints.Context.Discussion)]IUnitOfWork unitOfWork, 
        IValidator<PostMessageCommand> validator,
        IDiscussionRepository repository,
        IDateTimeProvider dateTimeProvider, 
        IPublisher publisher)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _publisher = publisher;
    }

    public async Task<Result<MessageId>> Handle(
        PostMessageCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            var discussionId = DiscussionId.Create(command.DiscussionId);
            var discussion = await _repository.GetById(discussionId, cancellationToken);
            if (discussion.IsFailure)
                return discussion.Errors;

            var messageId = MessageId.NewGuid();
            var text = Text.Create(command.Text).Value;
            var createdAt = CreatedAt.Create(_dateTimeProvider.UtcNow).Value;
            var isEdited = new IsEdited(false);
            var isRead = new IsRead(false);
            
            var message = Message.Create(messageId, text, createdAt, isEdited, isRead, command.UserId);
            if (message.IsFailure)
                return message.Errors;

            var result = discussion.Value.SendComment(message.Value);
            if (result.IsFailure)
                return result.Errors;

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();
            
            _logger.LogInformation("user with id {userId} post comment to discussion with id {discussionId}",
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