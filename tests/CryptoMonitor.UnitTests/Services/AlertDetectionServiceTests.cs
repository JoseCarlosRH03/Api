using CryptoMonitor.Application.Options;
using CryptoMonitor.Application.Services;
using CryptoMonitor.Application.Telemetry;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CryptoMonitor.UnitTests.Services;

public sealed class AlertDetectionServiceTests
{
    private readonly Mock<IAssetRepository> _assetRepoMock = new();
    private readonly Mock<IPriceHistoryRepository> _historyRepoMock = new();
    private readonly Mock<ICryptoMonitorMetrics> _metricsMock = new();

    private AlertDetectionService CreateService(double thresholdPercent = 5.0, int windowHours = 24) =>
        new(_assetRepoMock.Object, _historyRepoMock.Object,
            Options.Create(new PriceAlertOptions { ThresholdPercent = thresholdPercent, WindowHours = windowHours }),
            NullLogger<AlertDetectionService>.Instance,
            _metricsMock.Object);

    [Fact]
    public async Task AlertDetectionService_WhenPriceIncreasedOverThreshold_ReturnsAlert()
    {
        var asset = new Asset { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 106m, UpdatedAt = DateTimeOffset.UtcNow };
        _assetRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset> { asset });
        _historyRepoMock.Setup(r => r.GetBasePricesAsync(24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["bitcoin"] = 100m });

        var alerts = await CreateService().DetectAlertsAsync();

        alerts.Should().HaveCount(1);
        alerts[0].AssetId.Should().Be("bitcoin");
        alerts[0].VariationPercent.Should().BeApproximately(6.0, 0.01);
        _metricsMock.Verify(m => m.RecordAlertsDetected(1), Times.Once);
    }

    [Fact]
    public async Task AlertDetectionService_WhenPriceIncreasedBelowThreshold_ReturnsNoAlert()
    {
        var asset = new Asset { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 103m, UpdatedAt = DateTimeOffset.UtcNow };
        _assetRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset> { asset });
        _historyRepoMock.Setup(r => r.GetBasePricesAsync(24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["bitcoin"] = 100m });

        var alerts = await CreateService().DetectAlertsAsync();

        alerts.Should().BeEmpty();
        _metricsMock.Verify(m => m.RecordAlertsDetected(0), Times.Once);
    }

    [Fact]
    public async Task AlertDetectionService_WhenNoHistoryAvailable_ReturnsNoAlert()
    {
        var asset = new Asset { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 110m, UpdatedAt = DateTimeOffset.UtcNow };
        _assetRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Asset> { asset });
        _historyRepoMock.Setup(r => r.GetBasePricesAsync(24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        var alerts = await CreateService().DetectAlertsAsync();

        alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task AlertDetectionService_WithMultipleAssets_DetectsOnlyThoseThatExceedThreshold()
    {
        var assets = new List<Asset>
        {
            new() { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", PriceUsd = 110m, UpdatedAt = DateTimeOffset.UtcNow },
            new() { Id = "ethereum", Symbol = "ETH", Name = "Ethereum", PriceUsd = 102m, UpdatedAt = DateTimeOffset.UtcNow }
        };
        _assetRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(assets);
        _historyRepoMock.Setup(r => r.GetBasePricesAsync(24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>
            {
                ["bitcoin"] = 100m,   // +10% → alert
                ["ethereum"] = 100m   // +2%  → no alert
            });

        var alerts = await CreateService().DetectAlertsAsync();

        alerts.Should().HaveCount(1);
        alerts[0].AssetId.Should().Be("bitcoin");
        _metricsMock.Verify(m => m.RecordAlertsDetected(1), Times.Once);
    }
}
