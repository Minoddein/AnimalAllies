using AnimalAllies.Core.Common;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Messaging;
using AnimalAllies.Core.Options;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Volunteer.Application.Database;
using AnimalAllies.Volunteer.Application.Providers;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Infrastructure.BackgroundServices;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using AnimalAllies.Volunteer.Infrastructure.MessageQueues;
using AnimalAllies.Volunteer.Infrastructure.Options;
using AnimalAllies.Volunteer.Infrastructure.Providers;
using AnimalAllies.Volunteer.Infrastructure.Repository;
using AnimalAllies.Volunteer.Infrastructure.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;

namespace AnimalAllies.Volunteer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteerInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContexts()
            .AddMinio(configuration)
            .AddRepositories()
            .AddDatabase()
            .AddHostedServices()
            .AddServices(configuration);

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        services.AddSingleton<IMessageQueue<IEnumerable<FileInfo>>,
            InMemoryMessageQueue<IEnumerable<FileInfo>>>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EntityDeletion>(configuration.GetSection(EntityDeletion.ENTITY_DELETION));

        services.AddScoped<DeleteExpiredPetsService>();
        services.AddScoped<DeleteExpiredVolunteerService>();

        return services;
    }

    private static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<FilesCleanerBackgroundService>();
        services.AddHostedService<EntityCleanerIfDeletedBackgroundService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddKeyedScoped<IUnitOfWork, UnitOfWork>(Constraints.Context.PetManagement);
        services.AddKeyedScoped<ISqlConnectionFactory, SqlConnectionFactory>(Constraints.Context.PetManagement);

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddScoped<VolunteerWriteDbContext>();
        services.AddScoped<IReadDbContext, VolunteerReadDbContext>();

        return services;
    }

    private static IServiceCollection AddMinio(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.MINIO));

        services.AddMinio(options =>
        {
            MinioOptions minioOptions = configuration.GetSection(MinioOptions.MINIO)
                .Get<MinioOptions>() ?? throw new ApplicationException("Missing minio configuration");

            options.WithEndpoint(minioOptions.Endpoint);

            options.WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey);

            options.WithSSL(minioOptions.WithSsl);
        });

        services.AddScoped<IFileProvider, MinioProvider>();

        return services;
    }
}