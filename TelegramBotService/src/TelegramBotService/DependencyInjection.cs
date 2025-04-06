using MassTransit;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBotService.Consumers;
using TelegramBotService.Infrastructure;
using TelegramBotService.Infrastructure.Repository;
using TelegramBotService.Options;
using TelegramBotService.Services;
using TelegramBotService.States;
using TelegramBotService.States.Authorize;
using TelegramBotService.States.Info;
using TelegramBotService.States.Support;

namespace TelegramBotService;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureTelegramBotService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // Отключает 400 при ошибках валидации
            });
        
        
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddSingleton<UserStateServices>();

        services.AddTelegramBot(configuration);
        services.AddRedisCache(configuration);
        services.AddMessageBus(configuration);
        services.AddRepository();
        services.AddStates();
        services.AddDatabase(configuration);

        return services;
    }

    private static IServiceCollection AddStates(this IServiceCollection services)
    {
        services.AddScoped<StartState>();
        services.AddScoped<StartAuthorizeState>();
        services.AddScoped<WaitingForEmailState>();
        services.AddScoped<WaitingForPasswordState>();
        services.AddScoped<WaitingCommandState>();
        services.AddScoped<SupportState>();
        services.AddScoped<InfoState>();

        return services;
    }

    private static IServiceCollection AddRepository(this IServiceCollection services)
    {
        services.AddScoped<RedisUserStateRepository>();

        return services;
    }

    private static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.BOT));
        
        services.AddScoped<ITelegramBotClient>(config =>
        {
            var options = config.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
            var bot = new TelegramBotClient(options.Token);
            return bot;
        });

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ApplicationDbContext>();

        return services;
    }
    
    private static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<SendAuthorizationResponseEventConsumer>();
            configure.AddConsumer<SendTelegramNotificationEventConsumer>();
            
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"]!), h =>
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
    
    private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
      

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024 * 10; 
            options.MaximumKeyLength = 512;
         
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });
        return services;
    }
}