using System.Text.Json.Serialization;

namespace CryptoMonitor.Infrastructure.CoinCap.Models;

internal sealed record CoinCapAssetDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("priceUsd")] string? PriceUsd,
    [property: JsonPropertyName("marketCapUsd")] string? MarketCapUsd,
    [property: JsonPropertyName("volumeUsd24Hr")] string? VolumeUsd24Hr,
    [property: JsonPropertyName("changePercent24Hr")] string? ChangePercent24Hr
);
