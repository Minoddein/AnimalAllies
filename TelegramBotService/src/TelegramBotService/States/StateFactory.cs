namespace TelegramBotService.States;

using TelegramBotService.States.Authorize;

public static class StateFactory
{
    private static IServiceProvider _serviceProvider;
    private static IServiceScopeFactory _scopeFactory;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public static IState GetState(string stateName)
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("StateFactory is not initialized. Call Initialize() first.");
        }

        var stateType = Type.GetType(stateName);
        if (stateType == null || !typeof(IState).IsAssignableFrom(stateType))
        {
            throw new InvalidOperationException($"State '{stateName}' is not registered or does not implement IState.");
        }
        
        using var scope = _scopeFactory.CreateScope();
        var scopedServiceProvider = scope.ServiceProvider;
        
        return (IState)scopedServiceProvider.GetService(stateType)! 
               ?? throw new InvalidOperationException($"Failed to resolve state '{stateName}' from DI container.");
    }
}