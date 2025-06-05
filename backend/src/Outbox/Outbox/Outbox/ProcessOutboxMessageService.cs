using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using AnimalAllies.Accounts.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Outbox.Outbox;

public class ProcessOutboxMessageService
{
    private readonly OutboxContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessOutboxMessageService> _logger;
    private readonly ConcurrentDictionary<string, Type> _typeCache = new();
    private static readonly Assembly[] _contractAssemblies;

    static ProcessOutboxMessageService()
    {
        var baseDirectory = AppContext.BaseDirectory; 
        var contractAssemblies = Directory.GetFiles(
                baseDirectory, "*.Contracts.dll", SearchOption.AllDirectories)
            .Where(File.Exists) 
            .Select(Assembly.LoadFrom)
            .ToArray();

        var accountAssembly = AssemblyReference.Assembly;
        
        var allAssemblies = contractAssemblies.Concat([accountAssembly]);
        
        _contractAssemblies = allAssemblies.ToArray();
    }
    
    public ProcessOutboxMessageService(
        OutboxContext context, 
        IPublishEndpoint publishEndpoint, 
        ILogger<ProcessOutboxMessageService> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .OrderBy(m => m.OccurredOnUtc)
            .Where(m => m.ProcessedOnUtc == null)
            .Take(100)
            .ToListAsync(cancellationToken);
        
        if(messages.Count == 0)
            return;
        
        var pipeline = new ResiliencePipelineBuilder()
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
                },
            })
            .Build();

        var processingTasks = messages.Select(message =>
            ProcessMessageAsync(message, pipeline, cancellationToken));
        
        await Task.WhenAll(processingTasks);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
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
            var messageType = GetMessageType(message.Type);

            var deserializedMessage = JsonSerializer.Deserialize(message.Payload, messageType)
                                      ?? throw new NullReferenceException("Message payload not found");

            await pipeline.ExecuteAsync(
                async token =>
                {
                    await _publishEndpoint.Publish(deserializedMessage, messageType, token);
                    
                    message.ProcessedOnUtc = DateTime.UtcNow;
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            message.Error = ex.Message;
            message.ProcessedOnUtc = DateTime.UtcNow;
            _logger.LogError(ex, "Failed to process message ID: {MessageId}", message.Id);
        }
    }
    
    private Type GetMessageType(string typeName)
    {
        return _typeCache.GetOrAdd(typeName, name =>
        {
            var type = _contractAssemblies
                .Select(assembly => assembly.GetType(name))
                .FirstOrDefault(t => t != null);

            return type ?? throw new TypeLoadException($"Type '{name}' not found in any assembly");
        });
    }
} 