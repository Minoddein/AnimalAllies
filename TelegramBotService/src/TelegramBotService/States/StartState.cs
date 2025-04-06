using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.States.Authorize;

namespace TelegramBotService.States;

public class StartState: IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken = default)
    {
        var text =
            "Добро пожаловать в бота AnimalAllies! \ud83d\udc3e\n\n" +
            "/authorize\n" +
            "/info\n" +
            "/help\n";
        
        await botClient.SendMessage(
            message.Chat.Id, 
            text, 
            cancellationToken: cancellationToken);

        return new WaitingCommandState();
    }
}