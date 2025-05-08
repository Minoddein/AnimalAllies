using System.Security.Claims;
using AnimalAllies.Accounts.Application.Models;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Accounts.Application;

public interface ITokenProvider
{
    Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken = default);

    Task<Guid> GenerateRefreshToken(User user, Guid accessTokenJti, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Claim>>> GetUserClaimsFromJwtToken(
        string jwtToken,
        CancellationToken cancellationToken = default);
}