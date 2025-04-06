namespace NotificationService.Contracts.Requests;

public record SendTelegramNotificationEvent(Guid UserId, string Message);
