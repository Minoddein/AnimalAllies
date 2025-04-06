using MassTransit;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBotService.Contracts;
using TelegramBotService.Infrastructure;
using TelegramBotService.Models;

namespace TelegramBotService.Consumers;

public class SendAuthorizationResponseEventConsumer: IConsumer<SendAuthorizationResponseEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ITelegramBotClient _botClient;

    public SendAuthorizationResponseEventConsumer(ApplicationDbContext context, ITelegramBotClient botClient)
    {
        _context = context;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<SendAuthorizationResponseEvent> context)
    {
        var message = context.Message;
        
        if(message.UserId == Guid.Empty)
            return;

        var telegramUser = new TelegramUser
        {
            Id = Guid.NewGuid(),
            ChatId = message.ChatId,
            UserId = message.UserId,
        };

        var isExist =
            await _context.TelegramUsers.FirstOrDefaultAsync(u => u.ChatId == message.ChatId,
                context.CancellationToken);

        if (isExist is not null)
        {
            await _botClient.SendMessage(
                message.ChatId, 
                "Вы уже авторизованы!",
                cancellationToken: context.CancellationToken);
            
            return;
        }
            
        
        await _context.TelegramUsers.AddAsync(telegramUser, context.CancellationToken);
        await _context.SaveChangesAsync(context.CancellationToken);
        
        await _botClient.SendMessage(
            message.ChatId, 
            "Вы успешно авторизовались!",
            cancellationToken: context.CancellationToken);
    }
}