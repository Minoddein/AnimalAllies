using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Commands.RejectVolunteerRequest;

public class RejectVolunteerRequestHandler: ICommandHandler<RejectVolunteerRequestCommand, string>
{
    private readonly ILogger<RejectVolunteerRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IValidator<RejectVolunteerRequestCommand> _validator;
    private readonly IPublisher _publisher;

    public RejectVolunteerRequestHandler(
        ILogger<RejectVolunteerRequestHandler> logger, 
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork, 
        IVolunteerRequestsRepository repository, 
        IValidator<RejectVolunteerRequestCommand> validator,
        IPublisher publisher)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _validator = validator;
        _publisher = publisher;
    }
    
    public async Task<Result<string>> Handle(
        RejectVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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
            
            var volunteerRequest = await _repository.GetById(volunteerRequestId, cancellationToken);
            if (volunteerRequest.IsFailure)
                return volunteerRequest.Errors;

            if (volunteerRequest.Value.AdminId != command.AdminId)
                return Error.Failure("access.denied", 
                    "this request is under consideration by another admin");
            
            var rejectionComment = RejectionComment.Create(command.RejectionComment).Value;

            var rejectResult = volunteerRequest.Value.RejectRequest(rejectionComment);
            if (rejectResult.IsFailure)
                return rejectResult.Errors;
            
            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();
            
            _logger.LogInformation("Volunteer request with id {volunteerRequestId} was rejected",
                command.VolunteerRequestId);

            return rejectionComment.Value;
        }
        catch (Exception)
        {
            _logger.LogError("Fail to reject request");

            return Error.Failure("fail.reject.request", "Fail to reject request");
        }

    }
}