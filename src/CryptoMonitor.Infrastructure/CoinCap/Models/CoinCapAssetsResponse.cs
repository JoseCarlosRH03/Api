using System.Text.Json.Serialization;

namespace CryptoMonitor.Infrastructure.CoinCap.Models;

internal sealed record CoinCapAssetsResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<CoinCapAssetDto> Data
);
