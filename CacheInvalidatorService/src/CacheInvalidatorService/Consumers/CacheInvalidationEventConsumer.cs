using AnimalAllies.Accounts.Contracts.Events;
using CacheInvalidatorService.Services;
using MassTransit;

namespace CacheInvalidatorService.Consumers;

public class CacheInvalidationEventConsumer: IConsumer<CacheInvalidateIntegrationEvent>
{
    private readonly ILogger<CacheInvalidationEventConsumer> _logger;
    private readonly InvalidatorService _invalidatorService;

    public CacheInvalidationEventConsumer(
        ILogger<CacheInvalidationEventConsumer> logger,
        InvalidatorService invalidatorService)
    {
        _logger = logger;
        _invalidatorService = invalidatorService;
    }

    public async Task Consume(ConsumeContext<CacheInvalidateIntegrationEvent> context)
    {
        var message = context.Message;

        var tagOrKey = message.Key.Split('_').Length > 1 ? "key" : "tag";
        
        if(tagOrKey == "key")
            await _invalidatorService.InvalidateByKey(message.Key);
        else
            await _invalidatorService.InvalidateByTag(message.Key);
        
        _logger.LogInformation("User {key} has been invalidated", message.Key);
    }
}