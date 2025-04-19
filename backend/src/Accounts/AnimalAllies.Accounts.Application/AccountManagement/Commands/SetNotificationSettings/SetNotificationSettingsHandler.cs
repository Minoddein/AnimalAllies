using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;

public class SetNotificationSettingsHandler:  ICommandHandler<SetNotificationSettingsCommand>
{
    private readonly ILogger<SetNotificationSettingsHandler> _logger;
    private readonly IValidator<SetNotificationSettingsCommand> _validator;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox;
    private readonly IOutboxRepository _outboxRepository;

    public SetNotificationSettingsHandler(
        ILogger<SetNotificationSettingsHandler> logger,
        IValidator<SetNotificationSettingsCommand> validator,
        IUnitOfWorkOutbox unitOfWorkOutbox,
        IOutboxRepository outboxRepository)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWorkOutbox = unitOfWorkOutbox;
        _outboxRepository = outboxRepository;
    }

    public async Task<Result> Handle(
        SetNotificationSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();
        
        return Result.Success();
    }
}