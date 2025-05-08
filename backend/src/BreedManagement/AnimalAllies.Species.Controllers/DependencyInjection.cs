using AnimalAllies.Species.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Species.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddSpeciesPresentation(this IServiceCollection services)
    {
        services.AddContracts();

        return services;
    }

    private static IServiceCollection AddContracts(this IServiceCollection services)
    {
        services.AddScoped<ISpeciesContracts, SpeciesContracts>();

        return services;
    }
}