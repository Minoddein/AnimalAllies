using AnimalAllies.Accounts.Contracts.Events;
using CacheInvalidatorService.Services;
using MassTransit;

namespace CacheInvalidatorService.Consumers;

public class UserAddedAvatarEventConsumer: IConsumer<UserAddedAvatarIntegrationEvent>
{
    private readonly ILogger<UserAddedAvatarEventConsumer> _logger;
    private readonly InvalidatorService _invalidatorService;

    public UserAddedAvatarEventConsumer(
        ILogger<UserAddedAvatarEventConsumer> logger,
        InvalidatorService invalidatorService)
    {
        _logger = logger;
        _invalidatorService = invalidatorService;
    }

    public async Task Consume(ConsumeContext<UserAddedAvatarIntegrationEvent> context)
    {
        var message = context.Message;

        var key = $"users_{message.UserId}";

        await _invalidatorService.InvalidateByKey(key);
        
        _logger.LogInformation("User {MessageUserId} has been invalidated", message.UserId);
    }
}