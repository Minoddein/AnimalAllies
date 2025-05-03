using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;

public class RefreshTokensHandler: ICommandHandler<RefreshTokensCommand, LoginResponse>
{
    private readonly IRefreshSessionManager _refreshSessionManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IValidator<RefreshTokensCommand> _validator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokensHandler(
        IRefreshSessionManager refreshSessionManager,
        IValidator<RefreshTokensCommand> validator,
        IDateTimeProvider dateTimeProvider,
        ITokenProvider tokenProvider,
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork)
    {
        _refreshSessionManager = refreshSessionManager;
        _validator = validator;
        _dateTimeProvider = dateTimeProvider;
        _tokenProvider = tokenProvider;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<LoginResponse>> Handle(
        RefreshTokensCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var refreshSession = await _refreshSessionManager
            .GetByRefreshToken(command.RefreshToken, cancellationToken);
        
        if (refreshSession.IsFailure)
            return refreshSession.Errors;

        if (refreshSession.Value.ExpiresIn < _dateTimeProvider.UtcNow)
            return Errors.Tokens.ExpiredToken();
        
        await _refreshSessionManager.Delete(refreshSession.Value, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var accessToken = await _tokenProvider
            .GenerateAccessToken(refreshSession.Value.User,cancellationToken);
        var refreshToken = await _tokenProvider
            .GenerateRefreshToken(refreshSession.Value.User,accessToken.Jti, cancellationToken);

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            refreshSession.Value.UserId,
            refreshSession.Value.User.UserName!,
            refreshSession.Value.User.Email!,
            refreshSession.Value.User.ParticipantAccount!.FullName.FirstName,
            refreshSession.Value.User.ParticipantAccount.FullName.SecondName,
            refreshSession.Value.User.ParticipantAccount.FullName.Patronymic);
    }
}