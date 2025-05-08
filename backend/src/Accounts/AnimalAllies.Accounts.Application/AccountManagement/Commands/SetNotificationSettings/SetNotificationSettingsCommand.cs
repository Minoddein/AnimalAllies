using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;

public record SetNotificationSettingsCommand(
    Guid UserId,
    bool EmailNotifications,
    bool TelegramNotifications,
    bool WebNotifications) : ICommand;