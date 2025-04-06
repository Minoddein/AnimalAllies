using MediatR;
using Telegram.Bot;
using TelegramBotService.Commands;

namespace TelegramBotService.Handlers;

public class SendMessageHandler: IRequestHandler<SendMessageCommand>
{
    private ITelegramBotClient _botClient;

    public SendMessageHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        await _botClient.SendMessage(
            request.ChatId,
            request.Text,
            cancellationToken: cancellationToken
        );
    }
}