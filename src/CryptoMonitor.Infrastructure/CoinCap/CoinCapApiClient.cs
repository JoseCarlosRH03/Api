using System.Globalization;
using System.Net.Http.Json;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.CoinCap.Models;

namespace CryptoMonitor.Infrastructure.CoinCap;

internal sealed class CoinCapApiClient(HttpClient httpClient) : ICoinCapApiClient
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

    private static Asset MapToAsset(CoinCapAssetDto dto) => new()
    {
        Id = dto.Id,
        Symbol = dto.Symbol,
        Name = dto.Name,
        PriceUsd = ParseDecimal(dto.PriceUsd),
        MarketCapUsd = ParseDecimalOrNull(dto.MarketCapUsd),
        VolumeUsd24Hr = ParseDecimalOrNull(dto.VolumeUsd24Hr),
        ChangePercent24Hr = ParseDecimalOrNull(dto.ChangePercent24Hr),
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static decimal ParseDecimal(string? value) =>
        decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result : 0m;

    private static decimal? ParseDecimalOrNull(string? value) =>
        decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result : null;
}
