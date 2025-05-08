using AnimalAllies.Volunteer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteerPresentation(this IServiceCollection services)
    {
        services.AddContracts();

        return services;
    }

    private static IServiceCollection AddContracts(this IServiceCollection services)
    {
        services.AddScoped<IVolunteerContract, VolunteerContract>();

        return services;
    }
}