using System.Transactions;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateInfo;

public class UpdateInfoHandler : ICommandHandler<UpdateInfoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UpdateInfoHandler> _logger;
    private readonly IPublisher _publisher;

    public UpdateInfoHandler(
        [FromKeyedServices(Constraints.Context.Accounts)]
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<UpdateInfoHandler> logger,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<Result> Handle(
        UpdateInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            var user = await _userManager.Users
                .Include(u => u.ParticipantAccount)
                .Include(u => u.VolunteerAccount)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user?.VolunteerAccount is null && user?.ParticipantAccount is null)
                return Errors.General.NotFound();

            if (command.FirstName is not null 
                || command.SecondName is not null 
                || command.Patronymic is not null)
            {
                var newFirstName = command.FirstName ?? user?.ParticipantAccount!.FullName.FirstName;
                var newSecondName = command.SecondName ?? user?.ParticipantAccount!.FullName.SecondName;
                var newPatronymic = command.Patronymic ?? user?.ParticipantAccount!.FullName.Patronymic;

                var newFullName = FullName.Create(
                    newFirstName!,
                    newSecondName!,
                    newPatronymic);

                if (newFullName.IsFailure)
                {
                    return newFullName.Errors;
                }

                if (user?.ParticipantAccount is not null)
                {
                    user!.ParticipantAccount!.FullName = newFullName.Value;
                }

                if (user?.VolunteerAccount is not null)
                {
                    user.VolunteerAccount.FullName = newFullName.Value;
                }
            }

            if (command.Phone is not null)
            {
                var newPhone = PhoneNumber.Create(command.Phone);
                if(newPhone.IsFailure)
                    return newPhone.Errors;
                
                user.VolunteerAccount.Phone = newPhone.Value;
            }

            if (command.Experience is not null)
            {
                user.VolunteerAccount.Experience = command.Experience.Value;
            }
            
            var @event = new UserInfoUpdatedDomainEvent(user.Id);

            await _publisher.Publish(@event, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("Update info of user with id {id}", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating info to user with id {id}", command.UserId);

            return Error.Failure("fail.to.update.info",
                "Fail to update info to user");
        }
    }
}