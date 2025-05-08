using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Outbox.Outbox;

public class ProcessOutboxMessageService(
    OutboxContext context,
    IPublishEndpoint publishEndpoint,
    ILogger<ProcessOutboxMessageService> logger)
{
    private static readonly Assembly[] _contractAssemblies;
    private readonly OutboxContext _context = context;
    private readonly ILogger<ProcessOutboxMessageService> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ConcurrentDictionary<string, Type> _typeCache = new();

    static ProcessOutboxMessageService()
    {
        string baseDirectory = AppContext.BaseDirectory;
        Assembly[] contractAssemblies =
        [
            .. Directory.GetFiles(
                    baseDirectory, "*.Contracts.dll", SearchOption.AllDirectories)
                .Where(File.Exists)
                .Select(Assembly.LoadFrom)
        ];

        _contractAssemblies = contractAssemblies;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        List<OutboxMessage> messages = await _context
            .Set<OutboxMessage>()
            .OrderBy(m => m.OccurredOnUtc)
            .Where(m => m.ProcessedOnUtc == null)
            .Take(100)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (messages.Count == 0)
        {
            return;
        }

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnRetry = retryArguments =>
                {
                    _logger.LogCritical(
                        retryArguments.Outcome.Exception,
                        "Current attempt: {attemptNumber}",
                        retryArguments.AttemptNumber);

                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        IEnumerable<Task> processingTasks = messages.Select(message =>
            ProcessMessageAsync(message, pipeline, cancellationToken));

        await Task.WhenAll(processingTasks).ConfigureAwait(false);

        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changed to the database");
        }
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Type messageType = GetMessageType(message.Type);

            object deserializedMessage = JsonSerializer.Deserialize(message.Payload, messageType)
                                         ?? throw new NullReferenceException("Message payload not found");

            await pipeline.ExecuteAsync(
                async token =>
                {
                    await _publishEndpoint.Publish(deserializedMessage, messageType, token).ConfigureAwait(false);

                    message.ProcessedOnUtc = DateTime.UtcNow;
                }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            message.Error = ex.Message;
            message.ProcessedOnUtc = DateTime.UtcNow;
            _logger.LogError(ex, "Failed to process message ID: {MessageId}", message.Id);
        }
    }

    private Type GetMessageType(string typeName) =>
        _typeCache.GetOrAdd(typeName, name =>
        {
            Type? type = _contractAssemblies
                .Select(assembly => assembly.GetType(name))
                .FirstOrDefault(t => t != null);

            return type ?? throw new TypeLoadException($"Type '{name}' not found in any assembly");
        });
}