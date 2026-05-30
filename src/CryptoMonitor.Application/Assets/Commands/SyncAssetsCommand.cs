using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Assets.Commands;

public sealed record SyncAssetsCommand : IRequest<SyncResultDto>;

internal sealed class SyncAssetsCommandHandler(
    ICoinCapApiClient coinCapClient,
    IAssetRepository assetRepository,
    IPriceHistoryRepository priceHistoryRepository)
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

        return new SyncResultDto(assets.Count, syncedAt);
    }
}
