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

namespace VolunteerRequests.Application.Features.Commands.SendRequestForRevision;

public class SendRequestForRevisionHandler: ICommandHandler<SendRequestForRevisionCommand, VolunteerRequestId>
{
    private readonly ILogger<SendRequestForRevisionHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SendRequestForRevisionCommand> _validator;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IPublisher _publisher;
    
    public SendRequestForRevisionHandler(
        ILogger<SendRequestForRevisionHandler> logger, 
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork,
        IValidator<SendRequestForRevisionCommand> validator,
        IVolunteerRequestsRepository repository, 
        IPublisher publisher)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<VolunteerRequestId>> Handle(
        SendRequestForRevisionCommand command, CancellationToken cancellationToken = default)
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

            var rejectComment = RejectionComment.Create(command.RejectionComment).Value;

            var result = volunteerRequest.Value.SendRequestForRevision(rejectComment);
            if (result.IsFailure)
                return result.Errors;

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            
            scope.Complete();

            _logger.LogInformation("volunteer request with id {id} sent to revision", command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Something went wrong with sending request for revision");

            return Error.Failure("fail.to.send.to.revision.request",
                "Something went wrong with sending request for revision");
        }
    }
}