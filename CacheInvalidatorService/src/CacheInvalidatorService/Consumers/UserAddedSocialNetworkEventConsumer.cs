using AnimalAllies.Accounts.Contracts.Events;
using CacheInvalidatorService.Services;
using MassTransit;

namespace CacheInvalidatorService.Consumers;

public class UserAddedSocialNetworkEventConsumer: IConsumer<UserAddedSocialNetworkIntegrationEvent>
{
    private readonly ILogger<UserAddedSocialNetworkEventConsumer> _logger;
    private readonly InvalidatorService _invalidatorService;

    public UserAddedSocialNetworkEventConsumer(
        ILogger<UserAddedSocialNetworkEventConsumer> logger,
        InvalidatorService invalidatorService)
    {
        _logger = logger;
        _invalidatorService = invalidatorService;
    }

    public async Task Consume(ConsumeContext<UserAddedSocialNetworkIntegrationEvent> context)
    {
        var message = context.Message;

        var key = $"users_{message.UserId}";

        await _invalidatorService.InvalidateByKey(key);
        
        _logger.LogInformation("User {MessageUserId} has been invalidated", message.UserId);
    }
}