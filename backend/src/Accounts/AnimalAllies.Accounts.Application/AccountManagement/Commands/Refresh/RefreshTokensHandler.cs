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

public class RefreshTokensHandler : ICommandHandler<RefreshTokensCommand, LoginResponse>
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
        [FromKeyedServices(Constraints.Context.Accounts)]
        IUnitOfWork unitOfWork)
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
            .GenerateAccessToken(refreshSession.Value.User, cancellationToken);
        var refreshToken = await _tokenProvider
            .GenerateRefreshToken(refreshSession.Value.User, accessToken.Jti, cancellationToken);

        var roles = refreshSession.Value.User.Roles.Select(r => r.Name).ToArray();

        var permissions = refreshSession.Value.User.Roles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToArray();

        return InitLoginResponse(accessToken.AccessToken, refreshToken, refreshSession.Value.User);
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