using AnimalAllies.Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscussionApplication(this IServiceCollection services)
    {
        services
            .AddCommands()
            .AddQueries()
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
            .AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }

    private static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes
                .AssignableToAny([typeof(ICommandHandler<,>), typeof(ICommandHandler<>)]))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
    
    private static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IQueryHandler<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}