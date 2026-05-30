namespace CryptoMonitor.Domain.Entities;

public sealed class Asset
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public decimal PriceUsd { get; init; }
    public decimal? MarketCapUsd { get; init; }
    public decimal? VolumeUsd24Hr { get; init; }
    public decimal? ChangePercent24Hr { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
