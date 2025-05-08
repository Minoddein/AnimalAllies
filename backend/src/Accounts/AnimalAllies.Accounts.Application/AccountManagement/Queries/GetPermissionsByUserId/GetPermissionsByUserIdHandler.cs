using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetPermissionsByUserId;

public class GetPermissionsByUserIdHandler(
    ILogger<GetPermissionsByUserIdHandler> logger,
    IValidator<GetPermissionsByUserIdQuery> validator,
    IPermissionManager permissionManager,
    HybridCache hybridCache) : IQueryHandler<List<string>, GetPermissionsByUserIdQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetPermissionsByUserIdHandler> _logger = logger;
    private readonly IPermissionManager _permissionManager = permissionManager;
    private readonly IValidator<GetPermissionsByUserIdQuery> _validator = validator;

    public async Task<Result<List<string>>> Handle(
        GetPermissionsByUserIdQuery query,
        CancellationToken cancellationToken = default)
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

        Result<List<string>> cachePermission = await _hybridCache.GetOrCreateAsync(
            TagsConstants.PERMISSIONS + "_" + query.UserId,
            async _ =>
            {
                Result<List<string>> result =
                    await _permissionManager.GetPermissionsByUserId(query.UserId, cancellationToken)
                        .ConfigureAwait(false);

                return result.IsFailure ? result.Errors : result;
            },
            options,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (cachePermission.IsFailure)
        {
            return cachePermission.Errors;
        }

        _logger.LogInformation("Got permission by user id {id}", query.UserId);

        return cachePermission.Value;
    }
}