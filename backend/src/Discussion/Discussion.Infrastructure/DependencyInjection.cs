using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using Dapper;
using Discussion.Application.Repository;
using Discussion.Infrastructure.DbContexts;
using Discussion.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscussionInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase()
            .AddDbContexts()
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDiscussionRepository, DiscussionRepository>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddKeyedScoped<IUnitOfWork, UnitOfWork>(Constraints.Context.Discussion);
        services.AddKeyedSingleton<ISqlConnectionFactory, SqlConnectionFactory>(Constraints.Context.Discussion);

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddScoped<WriteDbContext>();

        return services;
    }
}