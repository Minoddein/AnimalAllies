using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AnimalAllies.Accounts.Application;
using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Application.Models;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Models;
using AnimalAllies.Core.Options;
using AnimalAllies.Framework;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AnimalAllies.Accounts.Infrastructure;

public class JwtTokenProvider(
    IOptions<JwtOptions> options,
    IDateTimeProvider dateTimeProvider,
    IOptions<RefreshSessionOptions> refreshSessionOptions,
    AccountsDbContext accountsDbContext,
    IPermissionManager permissionManager) : ITokenProvider
{
    private readonly AccountsDbContext _accountsDbContext = accountsDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly JwtOptions _jwtOptions = options.Value;
    private readonly IPermissionManager _permissionManager = permissionManager;
    private readonly RefreshSessionOptions _refreshSessionOptions = refreshSessionOptions.Value;

    public async Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken = default)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        IEnumerable<Claim> roleClaims = user.Roles
            .Select(r => new Claim(CustomClaims.Role, r.Name ?? string.Empty));

        Result<List<string>> permissions = await _permissionManager.GetPermissionsByUserId(user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (permissions.Value.Count == 0)
        {
            throw new ApplicationException("fail to load permission in jwt");
        }

        IEnumerable<Claim> permissionClaims = permissions.Value
            .Select(p => new Claim(CustomClaims.Permission, p));

        Guid jti = Guid.NewGuid();

        Claim[] claims =
        [
            new(CustomClaims.Id, user.Id.ToString()), new(CustomClaims.Email, user.Email!),
            new(CustomClaims.Username, user.UserName!), new(CustomClaims.Jti, jti.ToString())
        ];

        claims =
        [
            .. claims,
            .. roleClaims, .. permissionClaims
        ];

        JwtSecurityToken jwtToken = new(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_jwtOptions.ExpiredMinutesTime)),
            signingCredentials: signingCredentials,
            claims: claims);

        string? jwtStringToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return new JwtTokenResult(jwtStringToken, jti);
    }

    public async Task<Guid> GenerateRefreshToken(
        User user,
        Guid accessTokenJti,
        CancellationToken cancellationToken = default)
    {
        RefreshSession refreshSession = new()
        {
            User = user,
            CreatedAt = _dateTimeProvider.UtcNow,
            Jti = accessTokenJti,
            ExpiresIn = _dateTimeProvider.UtcNow.AddDays(_refreshSessionOptions.ExpiredDaysTime),
            RefreshToken = Guid.NewGuid()
        };

        await _accountsDbContext.RefreshSessions.AddAsync(refreshSession, cancellationToken).ConfigureAwait(false);
        await _accountsDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return refreshSession.RefreshToken;
    }

    public async Task<Result<IReadOnlyList<Claim>>> GetUserClaimsFromJwtToken(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        JwtSecurityTokenHandler jwtHandler = new();

        TokenValidationParameters validationParameters =
            TokenValidationParametersFactory.CreateWithoutLifeTime(_jwtOptions);

        TokenValidationResult? validationResult =
            await jwtHandler.ValidateTokenAsync(jwtToken, validationParameters).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return Errors.Tokens.InvalidToken();
        }

        return validationResult.ClaimsIdentity.Claims.ToList();
    }
}