using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Application.DTOs;

public record AssetDto(
    string Id,
    string Symbol,
    string Name,
    decimal PriceUsd,
    decimal? MarketCapUsd,
    decimal? VolumeUsd24Hr,
    decimal? ChangePercent24Hr,
    DateTimeOffset UpdatedAt
)
{
    public static AssetDto From(Asset asset) =>
        new(asset.Id,
            asset.Symbol,
            asset.Name,
            asset.PriceUsd,
            asset.MarketCapUsd,
            asset.VolumeUsd24Hr,
            asset.ChangePercent24Hr,
            asset.UpdatedAt
        );
}
