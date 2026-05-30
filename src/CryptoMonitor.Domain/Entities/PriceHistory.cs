namespace CryptoMonitor.Domain.Entities;

public sealed class PriceHistory
{
    public int Id { get; init; }
    public required string AssetId { get; init; }
    public decimal PriceUsd { get; init; }
    public DateTimeOffset RecordedAt { get; init; }

    public Asset? Asset { get; init; }
}
