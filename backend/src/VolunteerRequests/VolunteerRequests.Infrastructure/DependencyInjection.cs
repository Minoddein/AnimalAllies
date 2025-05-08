using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Infrastructure.DbContexts;
using VolunteerRequests.Infrastructure.Repository;

namespace VolunteerRequests.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteerRequestsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase()
            .AddDbContexts()
            .AddRepositories()
            .AddOutbox(configuration);

        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOutboxCore(configuration);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVolunteerRequestsRepository, VolunteerRequestsRepository>();
        services.AddScoped<IProhibitionSendingRepository, ProhibitionSendingRepository>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddKeyedScoped<IUnitOfWork, UnitOfWork>(Constraints.Context.VolunteerRequests);
        services.AddKeyedSingleton<ISqlConnectionFactory, SqlConnectionFactory>(Constraints.Context.VolunteerRequests);

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddScoped<WriteDbContext>();

        return services;
    }
}