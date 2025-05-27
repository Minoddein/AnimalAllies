using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Application.Repository;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Application.Features.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadHandler: ICommandHandler<MarkMessagesAsReadCommand>
{
    private readonly ILogger<MarkMessagesAsReadHandler> _logger;
    private readonly IValidator<MarkMessagesAsReadCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDiscussionRepository _discussionRepository;

    public MarkMessagesAsReadHandler(
        ILogger<MarkMessagesAsReadHandler> logger, 
        IValidator<MarkMessagesAsReadCommand> validator,
        [FromKeyedServices(Constraints.Context.Discussion)]IUnitOfWork unitOfWork,
        IDiscussionRepository discussionRepository)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _discussionRepository = discussionRepository;
    }

    public async Task<Result> Handle(
        MarkMessagesAsReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }
        
        var discussionId = DiscussionId.Create(command.DiscussionId);
        
        var discussion = await _discussionRepository.GetById(discussionId, cancellationToken);
        if (discussion.IsFailure)
        {
            return discussion.Errors;
        }
        
        var result = discussion.Value.MarkMessageAsRead(command.UserId);
        if (result.IsFailure)
            return result.Errors;

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation($"Marked message as read for user with id {command.UserId}");
        
        return Result.Success();
    }
}