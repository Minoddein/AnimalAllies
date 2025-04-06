using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotService.Infrastructure.Repository;
using TelegramBotService.Services;

namespace TelegramBotService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserStateServices _userStateService;
    private readonly ITelegramBotClient _botClient;
    private readonly RedisUserStateRepository _redisUserStateRepository;

    public AccountController(
        IMediator mediator,
        UserStateServices userStateService,
        ITelegramBotClient botClient,
        RedisUserStateRepository redisUserStateRepository)
    {
        _mediator = mediator;
        _userStateService = userStateService;
        _botClient = botClient;
        _redisUserStateRepository = redisUserStateRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        var message = update.Message;
        if (message == null) return Ok();

        var chatId = message.Chat.Id;
        var currentState = await _redisUserStateRepository.GetOrCreateStateAsync(chatId);
        var nextState = await currentState.HandleAsync(message, _botClient);
        await _redisUserStateRepository.SetStateAsync(chatId, nextState);

        return Ok();
    }
}