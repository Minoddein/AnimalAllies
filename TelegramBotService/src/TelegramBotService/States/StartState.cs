using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;
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
            "/support\n";
        
        await botClient.SendMessage(
            message.Chat.Id, 
            text, 
            cancellationToken: cancellationToken);

        return StateFactory.GetState(typeof(WaitingCommandState).FullName!);
    }
}