namespace CryptoMonitor.Application.DTOs;

public record AlertDto(
    string AssetId,
    string Symbol,
    decimal CurrentPriceUsd,
    decimal BasePriceUsd,
    double VariationPercent
);
