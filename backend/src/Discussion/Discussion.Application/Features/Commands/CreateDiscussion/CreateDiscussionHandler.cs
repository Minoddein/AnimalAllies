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
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.CreateDiscussion;

public class CreateDiscussionHandler(
    ILogger<CreateDiscussionHandler> logger,
    IValidator<CreateDiscussionCommand> validator,
    [FromKeyedServices(Constraints.Context.Discussion)]
    IUnitOfWork unitOfWork,
    IDiscussionRepository repository,
    IAccountContract accountContract,
    IPublisher publisher) : ICommandHandler<CreateDiscussionCommand, DiscussionId>
{
    private readonly IAccountContract _accountContract = accountContract;
    private readonly ILogger<CreateDiscussionHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IDiscussionRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateDiscussionCommand> _validator = validator;

    public async Task<Result<DiscussionId>> Handle(
        CreateDiscussionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? resultValidator =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!resultValidator.IsValid)
        {
            return resultValidator.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );
        try
        {
            Result<Domain.Aggregate.Discussion> isDiscussionExist =
                await _repository.GetByRelationId(command.RelationId, cancellationToken).ConfigureAwait(false);
            if (isDiscussionExist.IsSuccess)
            {
                return Errors.General.AlreadyExist();
            }

            bool firstMember = await _accountContract.IsUserExistById(command.FirstMember, cancellationToken)
                .ConfigureAwait(false);
            bool secondMember = await _accountContract.IsUserExistById(command.FirstMember, cancellationToken)
                .ConfigureAwait(false);

            if (!firstMember || !secondMember)
            {
                return Errors.General.NotFound(command.SecondMember);
            }

            Users users = Users.Create(command.FirstMember, command.SecondMember).Value;
            DiscussionId discussionId = DiscussionId.NewGuid();

            Result<Domain.Aggregate.Discussion> discussion =
                Domain.Aggregate.Discussion.Create(discussionId, users, command.RelationId);
            if (discussion.IsFailure)
            {
                return discussion.Errors;
            }

            await _repository.Create(discussion.Value, cancellationToken).ConfigureAwait(false);

            await _publisher.PublishDomainEvents(discussion.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation(
                "Created discussion for users with ids {id1} {id2}",
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
        CancellationToken cancellationToken = default) =>
        await Handle(new CreateDiscussionCommand(firstMember, secondMember, relationId), cancellationToken)
            .ConfigureAwait(false);
}