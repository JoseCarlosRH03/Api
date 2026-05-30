using CryptoMonitor.Application.Assets.Commands;
using CryptoMonitor.Application.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Infrastructure.BackgroundServices;

internal sealed class CoinCapSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<CoinCapOptions> options,
    ILogger<CoinCapSyncBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAsync(stoppingToken);
            await Task.Delay(
                TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes),
                stoppingToken);
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
                result.AssetsSynced, result.SyncedAt
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during asset sync");
        }
    }
}
