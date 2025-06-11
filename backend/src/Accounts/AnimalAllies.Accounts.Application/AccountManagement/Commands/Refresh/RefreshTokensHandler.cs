using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;

public class RefreshTokensHandler : ICommandHandler<RefreshTokensCommand, LoginResponse>
{
    private readonly IRefreshSessionManager _refreshSessionManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IValidator<RefreshTokensCommand> _validator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokensHandler> _logger;

    public RefreshTokensHandler(
        IRefreshSessionManager refreshSessionManager,
        IValidator<RefreshTokensCommand> validator,
        IDateTimeProvider dateTimeProvider,
        ITokenProvider tokenProvider,
        [FromKeyedServices(Constraints.Context.Accounts)]
        IUnitOfWork unitOfWork, 
        ILogger<RefreshTokensHandler> logger)
    {
        _refreshSessionManager = refreshSessionManager;
        _validator = validator;
        _dateTimeProvider = dateTimeProvider;
        _tokenProvider = tokenProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
    RefreshTokensCommand command, 
    CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrorList();
        
        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);
    
        try
        {
            var refreshSessionResult = await _refreshSessionManager
                .GetByRefreshToken(command.RefreshToken, cancellationToken);
    
            if (refreshSessionResult.IsFailure)
                return refreshSessionResult.Errors; 
    
            var refreshSession = refreshSessionResult.Value;
            
            if (refreshSession.ExpiresIn < _dateTimeProvider.UtcNow)
                return Errors.Tokens.ExpiredToken();
            
            if (refreshSession.User.IsBanned)
                return Error.Failure("user.banned", "Аккаунт заблокирован");
            
            var accessTokenResult = await _tokenProvider
                .GenerateAccessToken(refreshSession.User, cancellationToken);
            
            var refreshTokenResult = await _tokenProvider
                .GenerateRefreshToken(refreshSession.User, accessTokenResult.Jti, cancellationToken);
            
            if (refreshTokenResult == Guid.Empty)
            {
                return Error.Conflict("refreshTokenResult.Error", "Refresh token already deleted");
            }
            
            await _refreshSessionManager.Delete(refreshSession, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            transaction.Commit();
            
            return InitLoginResponse(
                accessTokenResult.AccessToken,
                refreshTokenResult,
                refreshSession.User);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Ошибка при обновлении токенов");
            return Error.Failure("refresh.failed", "Не удалось обновить токены");
        }
    }
    
    private LoginResponse InitLoginResponse(
        string accessToken,
        Guid refreshToken,
        User user)
    {
        var roles = user.Roles.Select(r => r.Name).ToArray();
        
        var permissions = user.Roles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToArray();

        var socialNetworks = user.SocialNetworks.Select(s =>
            new SocialNetworkResponse(s.Title, s.Url));

        var certificates = user.VolunteerAccount?.Certificates.Select(c =>
            new CertificateResponse(
                c.Title,
                c.IssuingOrganization,
                c.IssueDate,
                c.ExpirationDate, 
                c.Description));

        var requisites = user.VolunteerAccount?.Requisites.Select(r => 
            new RequisiteResponse(r.Title, r.Description));
        
        var response = new LoginResponse(
            accessToken,
            refreshToken,
            user.Id,
            user.UserName!,
            user.Email!,
            user.ParticipantAccount is not null 
                ? user.ParticipantAccount?.FullName.FirstName! 
                : string.Empty,
            user.ParticipantAccount is not null 
                ? user.ParticipantAccount.FullName.SecondName 
                : string.Empty,
            user.ParticipantAccount is not null 
                ? user.ParticipantAccount.FullName.Patronymic 
                : string.Empty,
            roles!,
            permissions,
            user.IsBanned,
            socialNetworks,
            user.VolunteerAccount is not null 
                ? new VolunteerAccountResponse(
                    user.VolunteerAccount.Id,
                    certificates!,
                    requisites!,
                    user.VolunteerAccount.Experience,
                    user.VolunteerAccount.Phone.Number)
                : null);

        return response;
    }
}