using System.Transactions;
using AnimalAllies.Accounts.Contracts;
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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.CreateDiscussion;

public class CreateDiscussionHandler: ICommandHandler<CreateDiscussionCommand, DiscussionId>
{
    private readonly ILogger<CreateDiscussionHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateDiscussionCommand> _validator;
    private readonly IDiscussionRepository _repository;
    private readonly IAccountContract _accountContract;
    private readonly IPublisher _publisher;

    public CreateDiscussionHandler(
        ILogger<CreateDiscussionHandler> logger, 
        IValidator<CreateDiscussionCommand> validator, 
        [FromKeyedServices(Constraints.Context.Discussion)]IUnitOfWork unitOfWork, 
        IDiscussionRepository repository,
        IAccountContract accountContract,
        IPublisher publisher)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _accountContract = accountContract;
        _publisher = publisher;
    }

    public async Task<Result<DiscussionId>> Handle(
        CreateDiscussionCommand command, CancellationToken cancellationToken = default)
    {
        var resultValidator = await _validator.ValidateAsync(command, cancellationToken);
        if (!resultValidator.IsValid)
            return resultValidator.ToErrorList();

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );
        try
        {
            var isDiscussionExist = await _repository.GetByRelationId(command.RelationId, cancellationToken);
            if (isDiscussionExist.IsSuccess)
                return Errors.General.AlreadyExist();

            var firstMember = await _accountContract.IsUserExistById(command.FirstMember, cancellationToken);
            var secondMember = await _accountContract.IsUserExistById(command.FirstMember, cancellationToken);

            if (!firstMember || !secondMember)
                return Errors.General.NotFound(command.SecondMember);

            var users = Users.Create(command.FirstMember, command.SecondMember).Value;
            var discussionId = DiscussionId.NewGuid();

            var discussion = Domain.Aggregate.Discussion.Create(discussionId, users, command.RelationId);
            if (discussion.IsFailure)
                return discussion.Errors;
            
            await _repository.Create(discussion.Value, cancellationToken);

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();

            _logger.LogInformation("Created discussion for users with ids {id1} {id2}",
                users.FirstMember, users.SecondMember);

            return discussionId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot create  discussion");
            
            return Error.Failure("fail.to.create.discussion", "Cannot create discussion");
        }
    }

    public async Task<Result<DiscussionId>> Handle(
        Guid firstMember,
        Guid secondMember,
        Guid relationId, 
        CancellationToken cancellationToken = default)
    {
        return await Handle(new CreateDiscussionCommand(firstMember, secondMember, relationId), cancellationToken);
    }
}