using CryptoMonitor.Application.Assets.Commands;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Application.Telemetry;
using CryptoMonitor.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Infrastructure.BackgroundServices;

internal sealed class CoinCapSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<CoinCapOptions> options,
    ILogger<CoinCapSyncBackgroundService> logger,
    ICryptoMonitorMetrics metrics)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DetectStartupGapAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAsync(stoppingToken);
            await Task.Delay(
                TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes),
                stoppingToken);
        }
    }

    private async Task DetectStartupGapAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();
        var lastSyncTime = await repository.GetLastSyncTimeAsync(cancellationToken);

        if (lastSyncTime is null)
        {
            logger.LogInformation("No prior sync history found — first startup or empty database.");
            return;
        }

        var gapMinutes = (DateTime.UtcNow - lastSyncTime.Value).TotalMinutes;
        var missedCycles = (int)Math.Floor(gapMinutes / options.Value.SyncIntervalMinutes);

        if (missedCycles >= 1)
        {
            logger.LogWarning(
                "Service restarted after {GapMinutes:F1} min of downtime — approximately {MissedCycles} sync cycle(s) were missed. " +
                "CoinCap does not provide retroactive price history; the gap in PriceHistory cannot be backfilled. " +
                "The next sync will capture current prices.",
                gapMinutes, missedCycles);

            metrics.RecordStartupGap(gapMinutes);
        }
        else
        {
            logger.LogDebug(
                "Service restarted. Gap since last sync: {GapMinutes:F1} min — within normal interval.",
                gapMinutes);
        }
    }

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var result = await mediator.Send(new SyncAssetsCommand(), cancellationToken);
            logger.LogInformation(
                "Synced {AssetCount} assets at {SyncedAt}",
                result.AssetsSynced, result.SyncedAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during asset sync");
        }
    }
}
