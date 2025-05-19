using System.Transactions;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateRequisites;

public class UpdateRequisitesHandler: ICommandHandler<UpdateRequisitesCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UpdateRequisitesHandler> _logger;
    private readonly IValidator<UpdateRequisitesCommand> _validator;
    private readonly IPublisher _publisher;

    public UpdateRequisitesHandler(
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<UpdateRequisitesHandler> logger, 
        IValidator<UpdateRequisitesCommand> validator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result> Handle(
        UpdateRequisitesCommand command,
        CancellationToken cancellationToken = default)
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
            var user = await _userManager.Users
                .Include(u => u.VolunteerAccount)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            
            if (user?.VolunteerAccount is null)
                return Errors.General.NotFound();

            var requisites = command.Requisites
                .Select(c => Requisite.Create(c.Title, c.Description).Value);

            user.VolunteerAccount.UpdateRequisites(requisites);

            var @event = new UserInfoUpdatedDomainEvent(user.Id);

            await _publisher.Publish(@event, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("Added certificates to user with id {id}", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding certificates to user with id {id}", command.UserId);

            return Error.Failure("fail.to.add.certificates", 
                "Fail to add certificates to user");
        }
    }
}