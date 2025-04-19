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

    public CreateDiscussionHandler(
        ILogger<CreateDiscussionHandler> logger, 
        IValidator<CreateDiscussionCommand> validator, 
        [FromKeyedServices(Constraints.Context.Discussion)]IUnitOfWork unitOfWork, 
        IDiscussionRepository repository,
        IAccountContract accountContract)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _accountContract = accountContract;
    }

    public async Task<Result<DiscussionId>> Handle(
        CreateDiscussionCommand command, CancellationToken cancellationToken = default)
    {
        var resultValidator = await _validator.ValidateAsync(command, cancellationToken);
        if (!resultValidator.IsValid)
            return resultValidator.ToErrorList();

        var isDiscussionExist = await _repository.GetByRelationId(command.RelationId,cancellationToken);
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
        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("Created discussion for users with ids {id1} {id2}",
            users.FirstMember, users.SecondMember);

        return discussionId;
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