using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;

public class IsUserExistByIdHandler(
    ILogger<IsUserExistByIdHandler> logger,
    IValidator<IsUserExistByIdQuery> validator,
    [FromKeyedServices(Constraints.Context.Accounts)]
    ISqlConnectionFactory sqlConnectionFactory,
    HybridCache hybridCache) : IQueryHandler<bool, IsUserExistByIdQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<IsUserExistByIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<IsUserExistByIdQuery> _validator = validator;

    public async Task<Result<bool>> Handle(IsUserExistByIdQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(8), LocalCacheExpiration = TimeSpan.FromMinutes(3)
        };

        IEnumerable<UserDto> cacheUser = await _hybridCache.GetOrCreateAsync(
            TagsConstants.USERS + "_" + query.UserId,
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();

                parameters.Add("@UserId", query.UserId);

                StringBuilder sql = new("""
                                        select
                                            u.id ,
                                            u.user_name
                                        from accounts.users u
                                        where u.id = @UserId limit 1
                                        """);

                return await connection.QueryAsync<UserDto>(sql.ToString(), parameters).ConfigureAwait(false);
            },
            options,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!cacheUser.Any())
        {
            return false;
        }

        _logger.LogInformation("User with id {QueryUserId} found", query.UserId);

        return true;
    }

    public async Task<Result<bool>> Handle(Guid userId, CancellationToken cancellationToken = default) =>
        await Handle(new IsUserExistByIdQuery(userId), cancellationToken).ConfigureAwait(false);
}