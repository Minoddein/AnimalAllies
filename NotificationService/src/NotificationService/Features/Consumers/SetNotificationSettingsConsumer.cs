using AnimalAllies.Accounts.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Models;
using NotificationService.Infrastructure.DbContext;

namespace NotificationService.Features.Consumers;

public class SetNotificationSettingsConsumer: IConsumer<SetNotificationSettingsCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SetStartUserNotificationSettingsEventConsumer> _logger;

    public SetNotificationSettingsConsumer(
        ApplicationDbContext dbContext,
        ILogger<SetStartUserNotificationSettingsEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SetNotificationSettingsCommand> context)
    {
        var message = context.Message;

        var userNotificationsSettingsEvent =
            await _dbContext.UserNotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == message.UserId, context.CancellationToken);

        if (userNotificationsSettingsEvent is null)
            return;

        userNotificationsSettingsEvent.EmailNotifications = message.EmailNotifications;
        userNotificationsSettingsEvent.WebNotifications = message.WebNotifications;
        userNotificationsSettingsEvent.TelegramNotifications = message.TelegramNotifications;
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("Set new notification settings for user {userid}", message.UserId);
    }
}