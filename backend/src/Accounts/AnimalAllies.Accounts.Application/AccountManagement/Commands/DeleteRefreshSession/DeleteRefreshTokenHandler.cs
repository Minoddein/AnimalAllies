using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;

public class DeleteRefreshTokenHandler(
    ILogger<DeleteRefreshTokenHandler> logger,
    IValidator<DeleteRefreshTokenCommand> validator,
    IRefreshSessionManager refreshSessionManager) : ICommandHandler<DeleteRefreshTokenCommand>
{
    private readonly ILogger<DeleteRefreshTokenHandler> _logger = logger;
    private readonly IRefreshSessionManager _refreshSessionManager = refreshSessionManager;
    private readonly IValidator<DeleteRefreshTokenCommand> _validator = validator;

    public async Task<Result> Handle(
        DeleteRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        Result<RefreshSession> refreshSession = await _refreshSessionManager
            .GetByRefreshToken(command.RefreshToken, cancellationToken).ConfigureAwait(false);

        if (refreshSession.IsFailure)
        {
            return refreshSession.Errors;
        }

        await _refreshSessionManager.Delete(refreshSession.Value, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("RefreshSession has been deleted");

        return Result.Success();
    }
}