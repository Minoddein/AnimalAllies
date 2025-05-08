namespace Outbox.Abstractions;

public interface IOutboxRepository
{
    Task AddAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
}