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

    public async Task<IReadOnlyList<AlertDto>> DetectAlertsAsync(int? windowHours = null, CancellationToken cancellationToken = default)
    {
        var effectiveWindow = windowHours ?? _options.WindowHours;
        var assets = await assetRepository.GetAllAsync(cancellationToken);
        var basePrices = await priceHistoryRepository.GetBasePricesAsync(effectiveWindow, cancellationToken);
        var alerts = new List<AlertDto>();

        foreach (var asset in assets)
        {
            if (!basePrices.TryGetValue(asset.Id, out var basePrice) || basePrice == 0)
            {
                logger.LogDebug("No base price for {AssetId} in {WindowHours}h — skipping", asset.Id, effectiveWindow);
                continue;
            }

            var variation = (double)((asset.PriceUsd - basePrice) / basePrice * 100);

            if (Math.Abs(variation) >= _options.ThresholdPercent)
            {
                logger.LogInformation("Alert detected for {AssetId}: {VariationPercent:F2}% in {WindowHours}h", asset.Id, variation, effectiveWindow);
                alerts.Add(new AlertDto(asset.Id, asset.Symbol, asset.PriceUsd, basePrice, variation));
            }
        }

        logger.LogDebug("Alert detection complete: {AlertCount} alert(s) from {AssetCount} asset(s) in {WindowHours}h window", alerts.Count, assets.Count, effectiveWindow);
        return alerts;
    }
}
