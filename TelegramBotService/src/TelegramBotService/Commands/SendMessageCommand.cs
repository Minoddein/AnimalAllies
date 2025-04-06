using MediatR;

namespace TelegramBotService.Commands;

public class SendMessageCommand: IRequest
{
    public long ChatId { get; set; }
    public string Text { get; set; }
}