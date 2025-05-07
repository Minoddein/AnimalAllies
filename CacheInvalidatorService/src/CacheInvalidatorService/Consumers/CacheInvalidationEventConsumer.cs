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

        if(message.Key is not null)
            await _invalidatorService.InvalidateByKey(message.Key);
        else if(message.Tags is not null)
            await _invalidatorService.InvalidateByTag(message.Tags);
        
        _logger.LogInformation("User {key} has been invalidated", message.Key);
    }
}