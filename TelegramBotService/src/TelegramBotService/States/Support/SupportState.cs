using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotService.States.Support;

public class SupportState: IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient, 
        CancellationToken cancellationToken = default)
    {
        await botClient.SendContact(
            message.Chat.Id, 
            "+79494814335",
            "Владислав",
            cancellationToken: cancellationToken );
        
        return StateFactory.GetState(typeof(SupportState).FullName!);
    }
}