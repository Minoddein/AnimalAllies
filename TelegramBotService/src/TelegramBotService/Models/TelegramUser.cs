namespace TelegramBotService.Models;

public class TelegramUser
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public Guid UserId { get; set; }
}