using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;

namespace TelegramBotService.States;

public interface IState
{
    Task<IState> HandleAsync(
        Message message, 
        ITelegramBotClient botClient,
        CancellationToken cancellationToken = default);
}
