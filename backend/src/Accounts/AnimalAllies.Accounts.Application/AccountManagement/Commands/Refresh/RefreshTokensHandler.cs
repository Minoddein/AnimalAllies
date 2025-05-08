using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Application.Models;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;

public class RefreshTokensHandler(
    IRefreshSessionManager refreshSessionManager,
    IValidator<RefreshTokensCommand> validator,
    IDateTimeProvider dateTimeProvider,
    ITokenProvider tokenProvider,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshTokensCommand, LoginResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IRefreshSessionManager _refreshSessionManager = refreshSessionManager;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<RefreshTokensCommand> _validator = validator;

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokensCommand command, CancellationToken cancellationToken = default)
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

        if (refreshSession.Value.ExpiresIn < _dateTimeProvider.UtcNow)
        {
            return Errors.Tokens.ExpiredToken();
        }

        await _refreshSessionManager.Delete(refreshSession.Value, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        JwtTokenResult accessToken = await _tokenProvider
            .GenerateAccessToken(refreshSession.Value.User, cancellationToken).ConfigureAwait(false);
        Guid refreshToken = await _tokenProvider
            .GenerateRefreshToken(refreshSession.Value.User, accessToken.Jti, cancellationToken).ConfigureAwait(false);

        string?[] roles = [.. refreshSession.Value.User.Roles.Select(r => r.Name)];

        string[] permissions =
        [
            .. refreshSession.Value.User.Roles
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission.Code)
        ];

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            refreshSession.Value.UserId,
            refreshSession.Value.User.UserName!,
            refreshSession.Value.User.Email!,
            refreshSession.Value.User.ParticipantAccount!.FullName.FirstName,
            refreshSession.Value.User.ParticipantAccount.FullName.SecondName,
            refreshSession.Value.User.ParticipantAccount.FullName.Patronymic,
            roles!,
            permissions);
    }
}