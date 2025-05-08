using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.Species.Application.Database;
using AnimalAllies.Species.Application.Repository;
using AnimalAllies.Species.Infrastructure.DbContexts;
using AnimalAllies.Species.Infrastructure.Repository;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox;

namespace AnimalAllies.Species.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSpeciesInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase()
            .AddDbContexts()
            .AddRepositories()
            .AddOutboxCore(configuration);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddKeyedScoped<IUnitOfWork, UnitOfWork>(Constraints.Context.BreedManagement);
        services.AddKeyedSingleton<ISqlConnectionFactory, SqlConnectionFactory>(Constraints.Context.BreedManagement);

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddScoped<SpeciesWriteDbContext>();
        services.AddScoped<IReadDbContext, SpeciesReadDbContext>();

        return services;
    }
}