using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;

public class SetNotificationSettingsHandler(
    ILogger<SetNotificationSettingsHandler> logger,
    IValidator<SetNotificationSettingsCommand> validator,
    IUnitOfWorkOutbox unitOfWorkOutbox,
    IOutboxRepository outboxRepository) : ICommandHandler<SetNotificationSettingsCommand>
{
    private readonly ILogger<SetNotificationSettingsHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox = unitOfWorkOutbox;
    private readonly IValidator<SetNotificationSettingsCommand> _validator = validator;

    public async Task<Result> Handle(
        SetNotificationSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        Contracts.Commands.SetNotificationSettingsCommand integrationCommand = new(
            command.UserId,
            command.EmailNotifications,
            command.TelegramNotifications,
            command.WebNotifications);

        await _outboxRepository.AddAsync(integrationCommand, cancellationToken).ConfigureAwait(false);

        await _unitOfWorkOutbox.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "sent integration command to set notification settings to user with id {id}",
            command.UserId);

        return Result.Success();
    }
}