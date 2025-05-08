using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;

public class DeleteRefreshTokenHandler: ICommandHandler<DeleteRefreshTokenCommand>
{
    private readonly ILogger<DeleteRefreshTokenHandler> _logger;
    private readonly IValidator<DeleteRefreshTokenCommand> _validator;
    private readonly IRefreshSessionManager _refreshSessionManager;

    public DeleteRefreshTokenHandler(
        ILogger<DeleteRefreshTokenHandler> logger,
        IValidator<DeleteRefreshTokenCommand> validator,
        IRefreshSessionManager refreshSessionManager)
    {
        _logger = logger;
        _validator = validator;
        _refreshSessionManager = refreshSessionManager;
    }

    public async Task<Result> Handle(
        DeleteRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var refreshSession = await _refreshSessionManager
            .GetByRefreshToken(command.RefreshToken, cancellationToken);
        
        if (refreshSession.IsFailure)
            return refreshSession.Errors;
        
        await _refreshSessionManager.Delete(refreshSession.Value, cancellationToken);

        _logger.LogInformation("RefreshSession has been deleted");
        
        return Result.Success();
    }
}