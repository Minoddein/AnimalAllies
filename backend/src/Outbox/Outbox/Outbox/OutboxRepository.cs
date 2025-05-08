using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Outbox.Abstractions;

namespace Outbox.Outbox;

public class OutboxRepository<TContext>(TContext context) : IOutboxRepository
    where TContext : DbContext
{
    private readonly TContext _context = context;

    public async Task AddAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        OutboxMessage outboxMessage = new()
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).FullName!,
            Payload = JsonSerializer.Serialize(message)
        };

        await _context.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken).ConfigureAwait(false);
    }
}