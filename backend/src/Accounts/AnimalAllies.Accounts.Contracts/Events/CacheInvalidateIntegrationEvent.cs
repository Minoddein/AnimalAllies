namespace AnimalAllies.Accounts.Contracts.Events;

public record CacheInvalidateIntegrationEvent(string? Key, IEnumerable<string>? Tags);