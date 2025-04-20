using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using MassTransit.Middleware.Outbox;
using Microsoft.AspNetCore.Mvc.Routing;
using NotificationService.Api.Extensions;
using NotificationService.Application.Abstraction;
using NotificationService.Features.Consumers;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.DbContext;
using NotificationService.Infrastructure.Services;
using NotificationService.Options;
using Outbox;
using Outbox.Abstractions;
using Outbox.Outbox;
using Serilog;
using EmailValidator = NotificationService.Validators.EmailValidator;

namespace NotificationService;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApp(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddExtensions(configuration)
            .AddMailConfiguration(configuration)
            .AddTelegramConfiguration()
            .AddInfrastructure()
            .AddMessageBus(configuration)
            .AddOutboxCore(configuration);
        
        return services;
    }

    private static IServiceCollection AddMailConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MailOptions>(
            configuration.GetSection(MailOptions.SECTION_NAME));
        services.AddScoped<EmailValidator>();
        services.AddScoped<MailSenderService>();

        services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
        
        return services;
    }

    private static IServiceCollection AddTelegramConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<TelegramBotHttpClient>();
            
        return services;
    }

    private static IServiceCollection AddExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackendUrlSettings>(configuration.GetSection(BackendUrlSettings.BACKEND));
        
        services.AddLogger(configuration);

        services.AddHttpLogging(o =>
        {
            o.CombineLogs = true;
        });

        services.AddSerilog();

        services.AddHangfire(options =>
        {
            options
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    c => 
                        c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
        });

        return services;
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<SetStartUserNotificationSettingsEventConsumer>();
            configure.AddConsumer<SendConfirmTokenByEmailEventConsumer>();
            configure.AddConsumer<ApproveVolunteerRequestEventConsumer>();
            configure.AddConsumer<CreateVolunteerRequestEventConsumer>();
            configure.AddConsumer<RejectVolunteerRequestEventConsumer>();
            configure.AddConsumer<TakeRequestForRevisionEventConsumer>();
            configure.AddConsumer<SetNotificationSettingsConsumer>();
            
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"] ??
                                 throw new ApplicationException("cannot host rabbitmq")), h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.Durable = true;

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
    
    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ApplicationDbContext>();
        services.AddScoped<IMigrator, Migrator>();

        return services;
    }
}