namespace TelegramBotService.Options;

public class TelegramBotOptions
{
    public static readonly string BOT = "BOT"; 
    
    public string Token { get; set; } = string.Empty;
    public string Ngrok { get; set; } = string.Empty;
}