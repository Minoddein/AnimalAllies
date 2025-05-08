using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Abstractions;
using Outbox.Outbox;
using Quartz;

namespace Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<OutboxContext>();
        services.AddOutbox();
        services.AddScoped<IUnitOfWorkOutbox, UnitOfWorkOutbox>();
        services.AddQuartzService();

        return services;
    }

    private static IServiceCollection AddOutbox(
        this IServiceCollection services)
    {
        services.AddScoped<IOutboxRepository, OutboxRepository<OutboxContext>>();

        services.AddScoped<ProcessOutboxMessageService>();

        return services;
    }

    private static IServiceCollection AddQuartzService(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            JobKey jobKey = new(nameof(ProcessOutboxMessageJob));

            configure.AddJob<ProcessOutboxMessageJob>(jobKey, configurator =>
                {
                    configurator.StoreDurably();
                })
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(1).RepeatForever()));
        });

        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

        return services;
    }
}