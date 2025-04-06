using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;

namespace TelegramBotService.States.Authorize;

public class StartAuthorizeState: IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken = default)
    {
        await botClient.SendMessage(
            message.Chat.Id, 
            "Добро пожаловать! Введите ваш email:", 
            cancellationToken: cancellationToken);

        return StateFactory.GetState(typeof(WaitingForEmailState).FullName!);
    }
}