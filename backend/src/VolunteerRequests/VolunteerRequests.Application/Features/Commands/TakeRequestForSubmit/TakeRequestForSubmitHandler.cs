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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;


namespace VolunteerRequests.Application.Features.Commands.TakeRequestForSubmit;

public class TakeRequestForSubmitHandler: ICommandHandler<TakeRequestForSubmitCommand>
{
    private readonly ILogger<TakeRequestForSubmitHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IDiscussionContract _discussionContract;
    private readonly IValidator<TakeRequestForSubmitCommand> _validator;
    private readonly IPublisher _publisher;

    public TakeRequestForSubmitHandler(
        ILogger<TakeRequestForSubmitHandler> logger, 
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork, 
        IVolunteerRequestsRepository repository, 
        IDiscussionContract discussionContract, 
        IValidator<TakeRequestForSubmitCommand> validator,
        IPublisher publisher)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _discussionContract = discussionContract;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result> Handle(
        TakeRequestForSubmitCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();
        
        
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );
        try
        {
            var volunteerRequestId = VolunteerRequestId.Create(command.VolunteerRequestId);
            
            var volunteerRequest = await _repository.GetById(volunteerRequestId, cancellationToken);
            if (volunteerRequest.IsFailure)
                return volunteerRequest.Errors;

            var discussionId = await _discussionContract.CreateDiscussionHandler(
                command.AdminId,
                volunteerRequest.Value.UserId,
                command.VolunteerRequestId,
                cancellationToken);

            var result = volunteerRequest.Value.TakeRequestForSubmit(command.AdminId, discussionId);
            if (result.IsFailure)
                return result.Errors;
            
            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();
            
            _logger.LogInformation("admin with id {id} take volunteer request for submit with id {volunteerRequestId}",
                command.AdminId, command.VolunteerRequestId);

            return Result.Success();
        }
        catch (Exception)
        {
            _logger.LogError("Cannot take request for submit");
            
            return Error.Failure("take.request.for.submit.failure",
                "Something went wrong with take request for submit");
        }
    }
}