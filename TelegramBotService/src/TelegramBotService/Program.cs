using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBotService;
using TelegramBotService.Options;
using TelegramBotService.Services;
using TelegramBotService.States;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureTelegramBotService(builder.Configuration);

var app = builder.Build();
StateFactory.Initialize(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var bot =  scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    var options = scope.ServiceProvider.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    
    await bot.SetWebhook($"{options.Ngrok}/api/Account");
}

app.Run();
