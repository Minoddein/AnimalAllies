using MassTransit;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Contracts;
using TelegramBotService.Infrastructure.Repository;
using TelegramBotService.Models;

namespace TelegramBotService.States.Authorize;

public class WaitingForPasswordState(RedisUserStateRepository repository, IPublishEndpoint publishEndpoint): IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken = default)
    {
        await repository.SetDataAsync(message.Chat.Id, message.Text, "password", cancellationToken);
        
        var email = await repository.GetOrCreateData<string>(message.Chat.Id, "email", cancellationToken);
        var password = await repository.GetOrCreateData<string>(message.Chat.Id, "password", cancellationToken);
        
        var messageEvent = new SendUserDataForAuthorizationEvent(message.Chat.Id, email, password);
        await publishEndpoint.Publish(messageEvent, cancellationToken);
        
        return StateFactory.GetState(typeof(WaitingCommandState).FullName!);
    }
}