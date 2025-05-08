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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.DeleteMessage;

public class DeleteMessageHandler: ICommandHandler<DeleteMessageCommand>
{
    private readonly ILogger<DeleteMessageHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteMessageCommand> _validator;
    private readonly IDiscussionRepository _repository;
    private readonly IPublisher _publisher;
    
    public DeleteMessageHandler(
        ILogger<DeleteMessageHandler> logger,
        [FromKeyedServices(Constraints.Context.Discussion)]IUnitOfWork unitOfWork,
        IValidator<DeleteMessageCommand> validator,
        IDiscussionRepository repository,
        IPublisher publisher)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(DeleteMessageCommand command, CancellationToken cancellationToken = default)
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

            var messageId = MessageId.Create(command.MessageId);

            var result = discussion.Value.DeleteComment(command.UserId, messageId);
            if (result.IsFailure)
                return result.Errors;

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();
            
            _logger.LogInformation("user with id {userId} delete message with id {messageId}" +
                                   " from discussion with id {discussionId}",
                command.UserId, command.MessageId, command.DiscussionId);

            return Result.Success();
        }
        catch (Exception)
        {
            _logger.LogError("Cannot delete message");
            
            return Error.Failure("cannot.delete.message", "Cannot delete message");
        }
    }
}