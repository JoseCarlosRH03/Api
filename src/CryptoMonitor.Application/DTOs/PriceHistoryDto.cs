using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Application.DTOs;

public record PriceHistoryDto(
    int Id,
    string AssetId,
    decimal PriceUsd,
    DateTime RecordedAt
)
{
    public static PriceHistoryDto From(PriceHistory history) =>
        new(history.Id,
            history.AssetId,
            history.PriceUsd,
            history.RecordedAt
        );
}
