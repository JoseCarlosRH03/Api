using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Domain.Entities;
using FluentAssertions;

namespace CryptoMonitor.UnitTests.DTOs;

public sealed class AssetDtoTests
{
    [Fact]
    public void AssetDto_From_MapsAllPropertiesCorrectly()
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var asset = new Asset
        {
            Id = "bitcoin",
            Symbol = "BTC",
            Name = "Bitcoin",
            PriceUsd = 50000.12m,
            MarketCapUsd = 1_000_000m,
            VolumeUsd24Hr = 500_000m,
            ChangePercent24Hr = 2.5m,
            UpdatedAt = updatedAt
        };

        var dto = AssetDto.From(asset);

        dto.Id.Should().Be(asset.Id);
        dto.Symbol.Should().Be(asset.Symbol);
        dto.Name.Should().Be(asset.Name);
        dto.PriceUsd.Should().Be(asset.PriceUsd);
        dto.MarketCapUsd.Should().Be(asset.MarketCapUsd);
        dto.VolumeUsd24Hr.Should().Be(asset.VolumeUsd24Hr);
        dto.ChangePercent24Hr.Should().Be(asset.ChangePercent24Hr);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
