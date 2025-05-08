using AnimalAllies.Accounts.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsPresentation(this IServiceCollection services)
    {
        services.AddContracts();

        return services;
    }

    private static IServiceCollection AddContracts(this IServiceCollection services)
    {
        services.AddScoped<IAccountContract, AccountContract>();

        return services;
    }
}