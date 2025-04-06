namespace TelegramBotService.Contexts;

public class UserContext
{
    public Guid Id { get; set; }
    public  long ChatId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}