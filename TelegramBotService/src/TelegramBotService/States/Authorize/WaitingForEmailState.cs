using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;

namespace TelegramBotService.States.Authorize;

public class WaitingForEmailState(RedisUserStateRepository repository): IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient, 
        CancellationToken cancellationToken = default)
    {
        await repository.SetDataAsync(message.Chat.Id, message.Text, "email", cancellationToken);
        
        await botClient.SendMessage(
            message.Chat.Id, 
            "Введите ваш пароль:", 
            cancellationToken: cancellationToken);

        return StateFactory.GetState(typeof(WaitingForPasswordState).FullName!);
    }
}