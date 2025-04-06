using Microsoft.Extensions.Caching.Hybrid;
using TelegramBotService.States;

namespace TelegramBotService.Infrastructure.Repository;

public class RedisUserStateRepository
{
    private readonly HybridCache _hybridCache;

    public RedisUserStateRepository(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task<IState> GetOrCreateStateAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var stateName = await _hybridCache.GetOrCreateAsync(
            $"user:{chatId}:state",
            factory: async _ => typeof(StartState).FullName,
            cancellationToken: cancellationToken);

        return StateFactory.GetState(stateName!);
    }

    public async Task SetStateAsync(long chatId, IState state, CancellationToken cancellationToken = default)
    {
        var stateName = state.GetType().FullName;

        if (string.IsNullOrEmpty(stateName))
        {
            throw new InvalidOperationException("State type name is null or empty.");
        }
        
        await _hybridCache.SetAsync(
            key: $"user:{chatId}:state",
            value: stateName,
            cancellationToken: cancellationToken);
    }

    public async Task<T> GetOrCreateData<T>(long chatId, string key, CancellationToken cancellationToken = default)
    {
        var data = await _hybridCache.GetOrCreateAsync(
            key: $"user:{chatId}:{key}",
            factory: async _ => default(T),
            cancellationToken: cancellationToken);
        
        if(data is null)
            throw new KeyNotFoundException($"Key {key} not found");

        return data;
    }
    
    public async Task SetDataAsync<T>(long chatId, T userData, string key, CancellationToken cancellationToken = default)
    {
        await _hybridCache.SetAsync(
            key: $"user:{chatId}:{key}",
            value: userData,
            cancellationToken: cancellationToken);
    }
}