namespace TelegramBotService.Contracts;

public record SendAuthorizationResponseEvent(long ChatId, Guid UserId);