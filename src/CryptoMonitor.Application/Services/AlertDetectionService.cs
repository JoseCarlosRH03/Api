using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Application.Interfaces;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Application.Services;

internal sealed class AlertDetectionService(
    IAssetRepository assetRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IOptions<PriceAlertOptions> options,
    ILogger<AlertDetectionService> logger)
    : IAlertDetectionService
{
    private readonly PriceAlertOptions _options = options.Value;

    public async Task<IReadOnlyList<AlertDto>> DetectAlertsAsync(CancellationToken cancellationToken = default)
    {
        var assets = await assetRepository.GetAllAsync(cancellationToken);
        var alerts = new List<AlertDto>();

        foreach (var asset in assets)
        {
            var basePrice = await priceHistoryRepository.GetBasePriceAsync(
                asset.Id, _options.WindowHours, cancellationToken);

            if (basePrice is null || basePrice == 0)
            {
                logger.LogDebug("No base price available for {AssetId} in the last {WindowHours}h — skipping", asset.Id, _options.WindowHours);
                continue;
            }

            var variation = (double)((asset.PriceUsd - basePrice.Value) / basePrice.Value * 100);

            if (Math.Abs(variation) >= _options.ThresholdPercent)
            {
                logger.LogInformation("Alert detected for {AssetId}: {VariationPercent:F2}% variation in {WindowHours}h", asset.Id, variation, _options.WindowHours);
                alerts.Add(new AlertDto(asset.Id, asset.Symbol, asset.PriceUsd, basePrice.Value, variation));
            }
        }

        logger.LogDebug("Alert detection complete: {AlertCount} alert(s) from {AssetCount} asset(s)", alerts.Count, assets.Count);
        return alerts;
    }
}
