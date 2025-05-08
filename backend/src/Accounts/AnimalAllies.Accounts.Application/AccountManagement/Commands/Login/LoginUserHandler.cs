using AnimalAllies.Accounts.Application.Models;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Login;

/// <summary>
///     Initializes a new instance of the <see cref="LoginUserHandler" /> class.
/// </summary>
/// <param name="userManager"></param>
/// <param name="logger"></param>
/// <param name="tokenProvider"></param>
/// <param name="validator"></param>
public class LoginUserHandler(
    UserManager<User> userManager,
    ILogger<LoginUserHandler> logger,
    ITokenProvider tokenProvider,
    IValidator<LoginUserCommand> validator) : ICommandHandler<LoginUserCommand, LoginResponse>
{
    private readonly ILogger<LoginUserHandler> _logger = logger;

    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IValidator<LoginUserCommand> _validator = validator;

    public async Task<Result<LoginResponse>> Handle(
        LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        User? user = await _userManager.Users
            .Include(u => u.Roles)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(u => u.ParticipantAccount)
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Errors.General.NotFound();
        }

        bool passwordConfirmed = await _userManager.CheckPasswordAsync(user, command.Password).ConfigureAwait(false);
        if (!passwordConfirmed)
        {
            return Errors.User.InvalidCredentials();
        }

        JwtTokenResult accessToken =
            await _tokenProvider.GenerateAccessToken(user, cancellationToken).ConfigureAwait(false);
        Guid refreshToken = await _tokenProvider.GenerateRefreshToken(user, accessToken.Jti, cancellationToken)
            .ConfigureAwait(false);

        string?[] roles = [.. user.Roles.Select(r => r.Name)];

        string[] permissions =
        [
            .. user.Roles
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission.Code)
        ];

        _logger.LogInformation("Successfully logged in");

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            user.Id,
            user.UserName!,
            user.Email!,
            user.ParticipantAccount!.FullName.FirstName,
            user.ParticipantAccount.FullName.SecondName,
            user.ParticipantAccount.FullName.Patronymic,
            roles!,
            permissions);
    }
}