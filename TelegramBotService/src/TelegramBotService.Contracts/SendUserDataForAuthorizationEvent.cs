namespace TelegramBotService.Contracts;

public record SendUserDataForAuthorizationEvent(long ChatId, string Email, string Password);