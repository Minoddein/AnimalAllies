using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;


namespace TelegramBotService;

public class ListenerBackgroundProcess : BackgroundService
{
    private readonly ILogger<ListenerBackgroundProcess> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ListenerBackgroundProcess(
        ILogger<ListenerBackgroundProcess> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        
        await botClient.DeleteWebhook(cancellationToken: stoppingToken);
        
        var redisUserStateRepository = scope.ServiceProvider.GetRequiredService<RedisUserStateRepository>();
        
        var updateHandler = new UpdateHandler(
            botClient,
            redisUserStateRepository,
            _logger
        );
        
        botClient.StartReceiving(
            updateHandler: updateHandler,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Бот начал прослушивание сообщений");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(200, stoppingToken);
        }
    }
}

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly RedisUserStateRepository _redisUserStateRepository;
    private readonly ILogger<ListenerBackgroundProcess> _logger;

    public UpdateHandler(
        ITelegramBotClient botClient,
        RedisUserStateRepository redisUserStateRepository,
        ILogger<ListenerBackgroundProcess> logger)
    {
        _botClient = botClient;
        _redisUserStateRepository = redisUserStateRepository;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            if (message == null) return;

            var chatId = message.Chat.Id;
            var currentState = await _redisUserStateRepository.GetOrCreateStateAsync(chatId, cancellationToken);
            var nextState = await currentState.HandleAsync(message, _botClient, cancellationToken);
            await _redisUserStateRepository.SetStateAsync(chatId, nextState, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке обновления");
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message);
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }

    public async Task HandlePollingErrorAsync(
        ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при polling");
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }
}