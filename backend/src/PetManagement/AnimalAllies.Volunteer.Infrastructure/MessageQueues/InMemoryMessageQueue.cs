using System.Threading.Channels;
using AnimalAllies.Core.Messaging;

namespace AnimalAllies.Volunteer.Infrastructure.MessageQueues;

public class InMemoryMessageQueue<TMessage> : IMessageQueue<TMessage>
{
    private readonly Channel<TMessage> _channel = Channel.CreateUnbounded<TMessage>();

    public async Task WriteAsync(TMessage message, CancellationToken cancellationToken = default) =>
        await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);

    public async Task<TMessage> ReadAsync(CancellationToken cancellationToken = default) =>
        await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
}