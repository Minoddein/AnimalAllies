using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;

public class IsUserExistByIdHandler: IQueryHandler<bool, IsUserExistByIdQuery>
{
    private const string REDIS_KEY = "users_";
    
    private readonly ILogger<IsUserExistByIdHandler> _logger;
    private readonly IValidator<IsUserExistByIdQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly HybridCache _hybridCache;

    public IsUserExistByIdHandler(
        ILogger<IsUserExistByIdHandler> logger,
        IValidator<IsUserExistByIdQuery> validator, 
        [FromKeyedServices(Constraints.Context.Accounts)]ISqlConnectionFactory sqlConnectionFactory,
        HybridCache hybridCache)
    {
        _logger = logger;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
        _hybridCache = hybridCache;
    }


    public async Task<Result<bool>> Handle(IsUserExistByIdQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(15)
        };

        var cacheUser = await _hybridCache.GetOrCreateAsync(
            key: REDIS_KEY + query.UserId,
            factory: async _ =>
            {
                var connection = _sqlConnectionFactory.Create();

                var parameters = new DynamicParameters();
        
                parameters.Add("@UserId", query.UserId);
        
                var sql = new StringBuilder("""
                                            select
                                                u.id ,
                                                u.user_name
                                            from accounts.users u
                                            where u.id = @UserId limit 1
                                            """);
                
                return await connection.QueryAsync<UserDto>(sql.ToString(), parameters);
            },
            options: options,
            cancellationToken: cancellationToken);

        if (!cacheUser.Any())
            return false;
        
        _logger.LogInformation("User with id {QueryUserId} found", query.UserId);
        
        return true;
    }

    public async Task<Result<bool>> Handle(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Handle(new IsUserExistByIdQuery(userId), cancellationToken);
    }
}