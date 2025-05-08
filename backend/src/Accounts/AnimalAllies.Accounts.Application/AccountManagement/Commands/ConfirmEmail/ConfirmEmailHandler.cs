using System.Transactions;
using AnimalAllies.Accounts.Application.Extensions;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.ConfirmEmail;

public class ConfirmEmailHandler(
    UserManager<User> userManager,
    ILogger<ConfirmEmailHandler> logger,
    IValidator<ConfirmEmailCommand> validator,
    IPublishEndpoint publishEndpoint,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWorkOutbox) : ICommandHandler<ConfirmEmailCommand>
{
    private readonly ILogger<ConfirmEmailHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox = unitOfWorkOutbox;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IValidator<ConfirmEmailCommand> _validator = validator;

    public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        User? user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return Errors.General.NotFound(command.UserId);
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, command.Code).ConfigureAwait(false);
        if (result.Errors.Any())
        {
            return result.Errors.ToErrorList();
        }

        SetStartUserNotificationSettingsEvent message = new(user.Id);

        await _outboxRepository.AddAsync(message, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);
        await _unitOfWorkOutbox.SaveChanges(cancellationToken).ConfigureAwait(false);

        scope.Complete();

        _logger.LogInformation("User {UserId} confirmed email.", command.UserId);

        return Result.Success();
    }
}