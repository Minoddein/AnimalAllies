using Discussion.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscussionPresentation(this IServiceCollection services)
    {
        services.AddContracts();

        return services;
    }

    private static IServiceCollection AddContracts(this IServiceCollection services)
    {
        services.AddScoped<IDiscussionContract, DiscussionContract>();

        return services;
    }
}