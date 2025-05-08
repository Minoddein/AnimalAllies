namespace AnimalAllies.Accounts.Contracts.Commands;

public record SetNotificationSettingsCommand(
    Guid UserId,
    bool EmailNotifications,
    bool TelegramNotifications,
    bool WebNotifications);