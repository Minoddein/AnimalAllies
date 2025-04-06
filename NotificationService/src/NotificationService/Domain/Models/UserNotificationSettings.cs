namespace NotificationService.Domain.Models;

public class UserNotificationSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool TelegramNotifications { get; set; }
    public bool WebNotifications { get; set; }
}