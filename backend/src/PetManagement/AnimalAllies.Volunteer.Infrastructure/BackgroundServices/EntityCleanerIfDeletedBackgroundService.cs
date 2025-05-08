using AnimalAllies.Volunteer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Infrastructure.BackgroundServices;

public class EntityCleanerIfDeletedBackgroundService(
    ILogger<FilesCleanerBackgroundService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const int FREQUENCY_OF_DELETION = 24;
    private readonly ILogger<FilesCleanerBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EntityCleanerIfDeletedBackgroundService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

            DeleteExpiredPetsService deleteExpiredPetsService =
                scope.ServiceProvider.GetRequiredService<DeleteExpiredPetsService>();
            DeleteExpiredVolunteerService deleteExpiredVolunteerService =
                scope.ServiceProvider.GetRequiredService<DeleteExpiredVolunteerService>();

            _logger.LogInformation("EntityCleanerIfDeletedBackgroundService is working");
            await deleteExpiredPetsService.Process(stoppingToken).ConfigureAwait(false);
            await deleteExpiredVolunteerService.Process(stoppingToken).ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromHours(FREQUENCY_OF_DELETION), stoppingToken).ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}