using CacheInvalidatorService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.ConfigureApplication(builder.Configuration);

var host = builder.Build();
host.Run();
