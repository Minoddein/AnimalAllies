using Microsoft.Extensions.DependencyInjection;
using Outbox.Abstractions;
using Outbox.Outbox;
using Quartz;

namespace Outbox;

public static class DependencyInjection
{
    /// <summary>
    /// Publisher
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOutboxPublisher(
        this IServiceCollection services)
    {
        services.AddScoped<OutboxContext>();
        services.AddScoped<IOutboxRepository, OutboxRepository<OutboxContext>>();
        services.AddScoped<IUnitOfWorkOutbox, UnitOfWorkOutbox>();
        
        return services;
    }

    /// <summary>
    /// Processor 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services)
    {
        services.AddScoped<ProcessOutboxMessageService>();
        services.AddQuartzService();
        
        return services;
    }
    
    private static IServiceCollection AddQuartzService(this IServiceCollection services) 
    {
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessageJob));

            configure.AddJob<ProcessOutboxMessageJob>(jobKey, configurator =>
                {
                    configurator.StoreDurably();
                })
                .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(1).RepeatForever()));
        });
        
        services.AddQuartzHostedService(options => {options.WaitForJobsToComplete = true;});
        
        return services; 
    }
}