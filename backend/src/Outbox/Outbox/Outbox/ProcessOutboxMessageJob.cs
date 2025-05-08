using Quartz;

namespace Outbox.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxMessageJob(ProcessOutboxMessageService processOutboxMessageService) : IJob
{
    private readonly ProcessOutboxMessageService _processOutboxMessageService = processOutboxMessageService;

    public async Task Execute(IJobExecutionContext context) =>
        await _processOutboxMessageService.Execute(context.CancellationToken).ConfigureAwait(false);
}