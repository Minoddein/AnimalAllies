using AnimalAllies.Core.Messaging;
using AnimalAllies.Volunteer.Application.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;

namespace AnimalAllies.Volunteer.Infrastructure.BackgroundServices;

public class FilesCleanerBackgroundService(
    ILogger<FilesCleanerBackgroundService> logger,
    IMessageQueue<IEnumerable<FileInfo>> messageQueue,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FilesCleanerBackgroundService is starting");

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        IFileProvider fileProvider = scope.ServiceProvider.GetRequiredService<IFileProvider>();

        while (!stoppingToken.IsCancellationRequested)
        {
            IEnumerable<FileInfo> fileInfos = await messageQueue.ReadAsync(stoppingToken).ConfigureAwait(false);

            foreach (FileInfo fileInfo in fileInfos)
            {
                await fileProvider.RemoveFile(fileInfo, stoppingToken).ConfigureAwait(false);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}