namespace AnimalAllies.Accounts.Contracts.Requests;

public record SetNotificationSettingsRequest(
    bool EmailNotifications,
    bool TelegramNotifications,
    bool WebNotifications);