using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Contracts.Requests;
using Telegram.Bot;
using TelegramBotService.Infrastructure;

namespace TelegramBotService.Consumers;

public class SendTelegramNotificationEventConsumer: IConsumer<SendTelegramNotificationEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ITelegramBotClient _botClient;

    public SendTelegramNotificationEventConsumer(
        ApplicationDbContext context,
        ITelegramBotClient botClient)
    {
        _context = context;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<SendTelegramNotificationEvent> context)
    {
        var message = context.Message;

        var telegramUser =
            await _context.TelegramUsers.FirstOrDefaultAsync(u => u.UserId == message.UserId,
                context.CancellationToken);
        
        if(telegramUser is null)
            return;
        
        await _botClient.SendMessage(
            telegramUser.ChatId,
            message.Message,
            cancellationToken: context.CancellationToken);
    }
}