﻿using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Login;

public class LoginUserHandler : ICommandHandler<LoginUserCommand,LoginResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<LoginUserHandler> _logger;

    private readonly ITokenProvider _tokenProvider;
    private readonly IValidator<LoginUserCommand> _validator;
    
    public LoginUserHandler(
        UserManager<User> userManager,
        ILogger<LoginUserHandler> logger,
        ITokenProvider tokenProvider,
        IValidator<LoginUserCommand> validator)
    {
        _userManager = userManager;
        _logger = logger;
        _tokenProvider = tokenProvider;
        _validator = validator;
    }
    
    public async Task<Result<LoginResponse>> Handle(
        LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();
        
        var user = await _userManager.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(u => u.ParticipantAccount)
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        
        if (user is null)
            return Errors.General.NotFound();

        var passwordConfirmed = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordConfirmed)
        {
            return Errors.User.InvalidCredentials();
        }

        var accessToken = await _tokenProvider.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenProvider.GenerateRefreshToken(user, accessToken.Jti,cancellationToken);
        
        var roles = user.Roles.Select(r => r.Name).ToArray();
        
        var permissions = user.Roles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToArray();

        _logger.LogInformation("Successfully logged in");
        
        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            user.Id,
            user.UserName!,
            user.Email!,
            user.ParticipantAccount is not null ? user.ParticipantAccount?.FullName.FirstName! : string.Empty,
            user.ParticipantAccount is not null ? user.ParticipantAccount.FullName.SecondName : string.Empty,
            user.ParticipantAccount is not null ? user.ParticipantAccount.FullName.Patronymic : string.Empty,
            roles!,
            permissions);
    }
}