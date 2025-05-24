using System.Transactions;
using AnimalAllies.Accounts.Contracts;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using VolunteerRequests.Application.Repository;

namespace VolunteerRequests.Application.Features.Commands.ApproveVolunteerRequest;

public class ApproveVolunteerRequestHandler: ICommandHandler<ApproveVolunteerRequestCommand>
{
    private readonly ILogger<ApproveVolunteerRequestHandler> _logger;
    private readonly IValidator<ApproveVolunteerRequestCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IPublisher _publisher;
    
    public ApproveVolunteerRequestHandler(
        ILogger<ApproveVolunteerRequestHandler> logger,
        IValidator<ApproveVolunteerRequestCommand> validator, 
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork, 
        IVolunteerRequestsRepository repository, 
        IPublisher publisher)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(
        ApproveVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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
            var volunteerRequestId = VolunteerRequestId.Create(command.VolunteerRequestId);
            
            var volunteerRequestResult = await _repository.GetById(volunteerRequestId, cancellationToken);
            if (volunteerRequestResult.IsFailure)
                return volunteerRequestResult.Errors;

            var volunteerRequest = volunteerRequestResult.Value;
            
            if (volunteerRequest.AdminId != command.AdminId)
                return Error.Failure("access.denied", 
                    "this request is under consideration by another admin");
            
            var approveResult = volunteerRequest.ApproveRequest();
            if (approveResult.IsFailure)
                return approveResult.Errors;
            
            await _publisher.PublishDomainEvents(volunteerRequest, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();
            
            _logger.LogInformation("Approved volunteer request with id {id}", command.VolunteerRequestId);

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError("Fail to approve volunteer request");

            return Error.Failure("fail.approve.request", "Fail to approve volunteer request");
        }
    }
}