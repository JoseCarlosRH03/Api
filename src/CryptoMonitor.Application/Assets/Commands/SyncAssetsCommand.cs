using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Application.Assets.Commands;

public sealed record SyncAssetsCommand : IRequest<SyncResultDto>;

internal sealed class SyncAssetsCommandHandler(
    ICoinCapApiClient coinCapClient,
    IAssetRepository assetRepository,
    IPriceHistoryRepository priceHistoryRepository,
    IOptions<PriceAlertOptions> options)
    : IRequestHandler<SyncAssetsCommand, SyncResultDto>
{
    public async Task<SyncResultDto> Handle(SyncAssetsCommand request, CancellationToken cancellationToken)
    {
        var assets = await coinCapClient.GetAssetsAsync(cancellationToken);

        await assetRepository.UpsertRangeAsync(assets, cancellationToken);

        var syncedAt = DateTime.UtcNow;
        var priceRecords = assets.Select(a => new PriceHistory
        {
            AssetId = a.Id,
            PriceUsd = a.PriceUsd,
            RecordedAt = syncedAt,
        });

        await priceHistoryRepository.AddRangeAsync(priceRecords, cancellationToken);

        if (options.Value.RetentionDays > 0)
        {
            var cutoff = syncedAt.AddDays(-options.Value.RetentionDays);
            await priceHistoryRepository.DeleteOlderThanAsync(cutoff, cancellationToken);
        }

        return new SyncResultDto(assets.Count, new DateTimeOffset(syncedAt, TimeSpan.Zero));
    }
}
