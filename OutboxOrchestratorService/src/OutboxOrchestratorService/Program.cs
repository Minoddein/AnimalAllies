using MassTransit;
using Outbox;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOutboxProcessor()
    .AddOutboxPublisher();

builder.Services.AddMassTransit(configure =>
{
    var configuration = builder.Configuration;
    
    configure.SetKebabCaseEndpointNameFormatter();
    
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

var host = builder.Build();
host.Run();