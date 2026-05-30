using System.Globalization;
using System.Net.Http.Json;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.CoinCap.Models;
using Microsoft.Extensions.Logging;

namespace CryptoMonitor.Infrastructure.CoinCap;

internal sealed class CoinCapApiClient(HttpClient httpClient, ILogger<CoinCapApiClient> logger) : ICoinCapApiClient
{
    public async Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .GetFromJsonAsync<CoinCapAssetsResponse>("assets", cancellationToken)
            .ConfigureAwait(false);

        if (response is null)
            return [];

        return response.Data
            .Where(d => d.PriceUsd is not null)
            .Select(MapToAsset)
            .ToList();
    }

    private Asset MapToAsset(CoinCapAssetDto dto) => new()
    {
        Id = dto.Id,
        Symbol = dto.Symbol,
        Name = dto.Name,
        PriceUsd = ParseDecimal(dto.Id, nameof(dto.PriceUsd), dto.PriceUsd),
        MarketCapUsd = ParseDecimalOrNull(dto.MarketCapUsd),
        VolumeUsd24Hr = ParseDecimalOrNull(dto.VolumeUsd24Hr),
        ChangePercent24Hr = ParseDecimalOrNull(dto.ChangePercent24Hr),
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private decimal ParseDecimal(string assetId, string field, string? value)
    {
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        logger.LogWarning("Failed to parse {Field} for asset {AssetId}: value was '{Value}' — defaulting to 0", field, assetId, value);
        return 0m;
    }

    private static decimal? ParseDecimalOrNull(string? value) =>
        decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result : null;
}
