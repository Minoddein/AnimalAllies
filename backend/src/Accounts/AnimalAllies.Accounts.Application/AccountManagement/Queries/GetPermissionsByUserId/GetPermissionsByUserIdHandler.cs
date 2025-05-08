using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetPermissionsByUserId;

public class GetPermissionsByUserIdHandler: IQueryHandler<List<string>, GetPermissionsByUserIdQuery>
{
    private readonly IPermissionManager _permissionManager;
    private readonly ILogger<GetPermissionsByUserIdHandler> _logger;
    private readonly IValidator<GetPermissionsByUserIdQuery> _validator;
    private readonly HybridCache _hybridCache;

    public GetPermissionsByUserIdHandler(
        ILogger<GetPermissionsByUserIdHandler> logger,
        IValidator<GetPermissionsByUserIdQuery> validator,
        IPermissionManager permissionManager,
        HybridCache hybridCache)
    {
        _logger = logger;
        _validator = validator;
        _permissionManager = permissionManager;
        _hybridCache = hybridCache;
    }
    
    public async Task<Result<List<string>>> Handle(
        GetPermissionsByUserIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(8),
            LocalCacheExpiration = TimeSpan.FromMinutes(3)
        };

        var cachePermission = await _hybridCache.GetOrCreateAsync(
            key: TagsConstants.PERMISSIONS + "_" + query.UserId,
            factory: async _ =>
            {
                var result = await _permissionManager.GetPermissionsByUserId(query.UserId, cancellationToken);
                
                return result.IsFailure ? result.Errors : result;
            },
            options: options,
            cancellationToken: cancellationToken);
        
        if (cachePermission.IsFailure)
            return cachePermission.Errors;

        _logger.LogInformation("Got permission by user id {id}", query.UserId);
        
        return cachePermission.Value;
    }
}