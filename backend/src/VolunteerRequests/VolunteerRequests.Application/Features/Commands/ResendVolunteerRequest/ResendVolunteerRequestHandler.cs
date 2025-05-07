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

namespace VolunteerRequests.Application.Features.Commands.ResendVolunteerRequest;

public class ResendVolunteerRequestHandler: ICommandHandler<ResendVolunteerRequestCommand, VolunteerRequestId>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResendVolunteerRequestHandler> _logger;
    private readonly IVolunteerRequestsRepository _repository;
    private readonly IValidator<ResendVolunteerRequestCommand> _validator;
    private readonly IPublisher _publisher;

    public ResendVolunteerRequestHandler(
        [FromKeyedServices(Constraints.Context.VolunteerRequests)]IUnitOfWork unitOfWork,
        ILogger<ResendVolunteerRequestHandler> logger,
        IVolunteerRequestsRepository repository,
        IValidator<ResendVolunteerRequestCommand> validator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _repository = repository;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result<VolunteerRequestId>> Handle(
        ResendVolunteerRequestCommand command, CancellationToken cancellationToken = default)
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

            if (volunteerRequest.Value.UserId != command.UserId)
                return Error.Failure("access.conflict", "Request belong another user!");

            var result = volunteerRequest.Value.ResendVolunteerRequest();
            if (result.IsFailure)
                return result.Errors;

            await _publisher.PublishDomainEvents(volunteerRequest.Value, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("user with id {userId} resent volunteer request with id {requestId}",
                command.UserId, command.VolunteerRequestId);

            return volunteerRequestId;
        }
        catch (Exception)
        {
            _logger.LogError("Cannot resend volunteer request");

            return Error.Failure("Fail.to.resend.volunteer.request",
                "Cannot resend volunteer request");
        }
    }
}